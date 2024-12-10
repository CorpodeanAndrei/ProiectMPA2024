using System.Security.Claims;

namespace ProiectMPA.Models
{
    public class UserClaimsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<Claim>? Claims { get; set; } // Listă de claim-uri (tip și valoare)
    }

}
