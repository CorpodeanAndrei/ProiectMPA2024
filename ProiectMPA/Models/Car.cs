using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectMPA.Models
{
    public class Car
    {
        public int ID { get; set; }
        public string Model { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }
        public int Year { get; set; }
        public int? ChasisTypeID { get; set; }
        public ChasisType? ChasisType { get; set; }
        public int? ManufacturerID { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public string ImagePath { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
