namespace EventosSernaJrAPI.Models.DTOs
{
    public class CreateOrderRequest
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public TimeSpan DeliveryTime { get; set; }
        public DateTime PickupDateTime { get; set; }
        public List<OrderProductRequest> Products { get; set; }
    }

    public class OrderProductRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
