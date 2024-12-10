namespace ProiectMPA.Models
{
    public class Manufacturer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<Car>? Cars { get; set; }
    }
}
