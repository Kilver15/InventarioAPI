namespace EventosSernaJrAPI.Models
{
    public class Log
    {
        public int Id { get; set; }
         public int UserId { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
    }
}
