namespace EventosSernaJrAPI.Models
{
    public class Order
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; } 
        public string Address { get; set; }
        public DateTime OrderDate { get; set; } 
        public TimeSpan DeliveryTime { get; set; }
        public DateTime PickupDateTime { get; set; }

        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
