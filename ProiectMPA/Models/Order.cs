namespace ProiectMPA.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int? ClientID { get; set; }
        public int? CarID { get; set; }
        public DateTime OrderDate { get; set; }

        public Client? Client { get; set; }
        public Car? Car { get; set; }
    }
}
