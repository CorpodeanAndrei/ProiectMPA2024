namespace ProiectMPA.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
