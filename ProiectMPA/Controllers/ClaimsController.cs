using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ProiectMPA.Models;
using Microsoft.EntityFrameworkCore;

namespace ProiectMPA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClaimsController : Controller
    {
        private UserManager<IdentityUser> userManager;
        public ClaimsController(UserManager<IdentityUser> userMgr)
        {
            userManager = userMgr;
        }
        public async Task<IActionResult> Index()
        {
            var users = userManager.Users;
            var userClaimsViewModels = new List<UserClaimsViewModel>();

            foreach (var user in users)
            {
                var claims = await userManager.GetClaimsAsync(user);
                userClaimsViewModels.Add(new UserClaimsViewModel
                {
                    UserName = user.UserName,
                    UserId = user.Id,
                    Claims = claims.ToList()
                });
            }

            return View(userClaimsViewModels);
        }



        public ViewResult Create() => View();


        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> Create_Post(string claimType, string claimValue)
        {
            IdentityUser user = await userManager.GetUserAsync(HttpContext.User);
            Claim claim = new Claim(claimType, claimValue, ClaimValueTypes.String);
            IdentityResult result = await userManager.AddClaimAsync(user, claim);
            if (result.Succeeded)
                return RedirectToAction("Index");
            else
                Errors(result);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string claimValues)
        {
            IdentityUser user = await userManager.GetUserAsync(HttpContext.User);
            string[] claimValuesArray = claimValues.Split(";");
            string claimType = claimValuesArray[0], claimValue = claimValuesArray[1], claimIssuer = claimValuesArray[2];
            Claim claim = User.Claims.Where(x => x.Type == claimType && x.Value == claimValue && x.Issuer == claimIssuer).FirstOrDefault();
            IdentityResult result = await userManager.RemoveClaimAsync(user, claim);
            if (result.Succeeded)
                return RedirectToAction("Index");
            else
                Errors(result);
            return View("Index");
        }

        void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        [HttpPost]
        public async Task<IActionResult> AddClaim(string userId, string claimType, string claimValue)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var claim = new Claim(claimType, claimValue);
            var result = await userManager.AddClaimAsync(user, claim);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveClaim(string userId, string claimType, string claimValue)
        {
            // Get the user
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Find the claim to remove
            var claim = new Claim(claimType, claimValue);
            var result = await userManager.RemoveClaimAsync(user, claim);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove claim.");
            }

            // Redirect to the same page with updated claims
            return RedirectToAction("AssignClaims", new { userId = userId });
        }



        public async Task<IActionResult> UserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var claims = await userManager.GetClaimsAsync(user);
            ViewBag.UserName = user.UserName;
            return View(claims);
        }

        public async Task<IActionResult> AssignClaims(string userId)
        {
            // Get the user by ID
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get the claims for the selected user
            var claims = await userManager.GetClaimsAsync(user);

            // Create the AssignClaimsViewModel and populate it
            var model = new AssignClaimsViewModel
            {
                UserId = userId,
                Claims = claims.Select(c => new ClaimViewModel
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            // Return the view with the model
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AssignClaims(string userId, string selectedClaimType, string selectedClaimValue)
        {
            // Find the user
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Add a new claim
            if (!string.IsNullOrEmpty(selectedClaimType) && !string.IsNullOrEmpty(selectedClaimValue))
            {
                var claim = new Claim(selectedClaimType, selectedClaimValue);
                var result = await userManager.AddClaimAsync(user, claim);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add claim.");
                    return View();
                }
            }

            // Get the updated list of claims for the user
            var claims = await userManager.GetClaimsAsync(user);

            // Pass the user claims to the view again
            var model = new AssignClaimsViewModel
            {
                UserId = userId,
                Claims = claims.Select(c => new ClaimViewModel
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            // Redirect to the same page (AssignClaims) with the updated data
            return View(model);
        }



        public async Task<IActionResult> Update(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Obținem claim-urile utilizatorului
            var claims = await userManager.GetClaimsAsync(user);

            var model = new UpdateClaimsViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Claims = claims.Select(c => new ClaimViewModel
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(string userId, string claimType, string claimValue)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove the claim
            var existingClaim = (await userManager.GetClaimsAsync(user))
                                .FirstOrDefault(c => c.Type == claimType && c.Value == claimValue);

            if (existingClaim != null)
            {
                var result = await userManager.RemoveClaimAsync(user, existingClaim);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove claim.");
                    return View();
                }
            }

            // Add the new claim if needed
            if (!string.IsNullOrEmpty(claimType) && !string.IsNullOrEmpty(claimValue))
            {
                var claim = new Claim(claimType, claimValue);
                var result = await userManager.AddClaimAsync(user, claim);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add claim.");
                    return View();
                }
            }

            // Redirect to Index after successful update
            return RedirectToAction("Index");
        }



    }
}