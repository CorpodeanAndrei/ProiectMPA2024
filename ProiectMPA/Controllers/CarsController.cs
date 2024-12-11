using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GRPCService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProiectMPA.Data;
//using ProiectMPA.Migrations;
using ProiectMPA.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace ProiectMPA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CarsController : Controller
    {
        private readonly GrpcChannel _channel;
        private UserManager<IdentityUser> _userManager;
        private readonly ProiectMPAContext _context;

        public CarsController(ProiectMPAContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _channel = GrpcChannel.ForAddress("https://localhost:7152");
        }

        // GET: Cars
        [AllowAnonymous]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["ModelSortParm"] = String.IsNullOrEmpty(sortOrder) ? "model_desc" : "Model";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["YearSortParm"] = sortOrder == "Year" ? "year_desc" : "Year";
            ViewData["ManufacturerSortParm"] = sortOrder == "Manufacturer" ? "manufacturer_desc" : "Manufacturer";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var cars = from c in _context.Car
                        join m in _context.Manufacturer on c.ManufacturerID equals m.ID
                        select new CarViewModel
                        {
                            ID = c.ID,
                            Model = c.Model,
                            Price = c.Price,
                            Year = c.Year,
                            ImagePath = c.ImagePath,
                            Manufacturer = m.Name,
                        };

            if (!String.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(s =>
                    s.Model.Contains(searchString) ||
                    s.Year.ToString().Contains(searchString) ||
                    s.Manufacturer.Contains(searchString)
                );
            }

            switch (sortOrder)
            {
                case "model_desc":
                    cars = cars.OrderByDescending(c => c.Model);
                    break;
                case "Price":
                    cars = cars.OrderBy(c => c.Price);
                    break;
                case "price_desc":
                    cars = cars.OrderByDescending(c => c.Price);
                    break;
                case "Year":
                    cars = cars.OrderBy(c => c.Year);
                    break;
                case "year_desc":
                    cars = cars.OrderByDescending(c => c.Year);
                    break;
                case "Manufacturer":
                    cars = cars.OrderBy(c => c.Manufacturer);
                    break;
                case "manufacturer_desc":
                    cars = cars.OrderByDescending(c => c.Manufacturer);
                    break;
                default:
                    cars = cars.OrderBy(c => c.Model);
                    break;
            }
            int pageSize = 20;

            return View(await PaginatedList<CarViewModel>.CreateAsync(cars.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Cars/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.ChasisType)
                .Include(c => c.Manufacturer)
                //.Include(s => s.Orders)
                //.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            ViewData["ChasisTypeID"] = new SelectList(_context.Set<ChasisType>(), "ID", "Name");
            ViewData["ManufacturerID"] = new SelectList(_context.Set<Manufacturer>(), "ID", "Name");
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Model,Price,Year,ChasisTypeID,ManufacturerID,ImagePath")] Car car)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(car);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ChasisTypeID"] = new SelectList(_context.Set<ChasisType>(), "ID", "Name", car.ChasisTypeID);
                ViewData["ManufacturerID"] = new SelectList(_context.Set<Manufacturer>(), "ID", "Name", car.ManufacturerID);
                
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. " + "Try again, and if the problem persists ");
            }
            return View(car);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            ViewData["ChasisTypeID"] = new SelectList(_context.Set<ChasisType>(), "ID", "Name", car.ChasisTypeID);
            ViewData["ManufacturerID"] = new SelectList(_context.Set<Manufacturer>(), "ID", "Name", car.ManufacturerID);
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Model,Price,Year,ChasisTypeID,ManufacturerID")] Car car)
        {
            if (id != car.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChasisTypeID"] = new SelectList(_context.Set<ChasisType>(), "ID", "Name", car.ChasisTypeID);
            ViewData["ManufacturerID"] = new SelectList(_context.Set<Manufacturer>(), "ID", "Name", car.ManufacturerID);
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.ChasisType)
                .Include(c => c.Manufacturer)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Car.FindAsync(id);
            if (car != null)
            {
                _context.Car.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Car.Any(e => e.ID == id);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int carId)
        {
            // Get the car from the database
            var car = await _context.Car
                .Include(c => c.Manufacturer)
                .FirstOrDefaultAsync(c => c.ID == carId);

            if (car == null)
            {
                ModelState.AddModelError("", "Mașina nu a fost găsită.");
                return RedirectToAction("Index", "Cars");
            }

            try
            {
                // Get the current logged-in user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError("", "Trebuie să fiți autentificat pentru a plasa o comandă.");
                    return RedirectToAction("Index", "Cars");
                }

                // Check if a Client exists for the current user
                //var client = await _context.Client.FirstOrDefaultAsync(c => c.UserId == user.Id);

                // If client does not exist, create it
                //if (client == null)
                //{
                //    client = new Client
                //    {
                //        Name = user.UserName,  // Use any other field you want, like Email
                //        UserId = user.Id
                //    };

                //    _context.Client.Add(client);
                //    await _context.SaveChangesAsync();  // Save the client to the database

                //    // Debug: Check if client was saved
                //    Console.WriteLine($"Clientul a fost salvat cu ID {client.Id}");
                //}

                // Create the order for the gRPC service
                //var orderClient = new OrderService.OrderServiceClient(_channel);
                //var grpcOrder = new GRPCService.Order
                //{
                //    OrderId = 0,  // ID-ul comenzii va fi generat pe server sau de gRPC
                //    CarId = carId,
                //    ClientId = client.Id,  // Associate with the client ID
                //    OrderDate = DateTime.Now.ToString("yyyy-MM-dd")
                //};

                //// Debug: Verify order details
                //Console.WriteLine($"Creăm order pentru carId {carId} și clientId {client.Id}.");

                //// Send the order to the gRPC server
                //var response = orderClient.Insert(grpcOrder);

                // Debug: Check response from gRPC
                //Console.WriteLine($"Comanda a fost trimisă cu succes. Răspuns: {response}");

                TempData["Message"] = "Comanda a fost plasată cu succes!";
                return RedirectToAction("Index", "Cars");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Eroare la trimiterea comenzii: {ex.Message}");
                ModelState.AddModelError("", $"Eroare la crearea comenzii: {ex.Message}");
                return RedirectToAction("Index", "Cars");
            }
        }

    }
}
