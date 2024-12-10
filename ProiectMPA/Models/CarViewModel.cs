using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProiectMPA.Models
{
    public class CarViewModel
    {
        public int ID { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public int Year { get; set; }
        [Display(Name = "Imagine")]

        public string ImagePath { get; set; }
        public string Manufacturer { get; set; }
    }
}
