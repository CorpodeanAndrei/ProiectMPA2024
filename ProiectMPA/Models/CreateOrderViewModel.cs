using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProiectMPA.Models
{
    public class CreateOrderViewModel
    {
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public int ClientId { get; set; }
        public List<SelectListItem>? Clients { get; set; }
    }
}
