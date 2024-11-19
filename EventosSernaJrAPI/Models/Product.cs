using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosSernaJrAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int InInventory { get; set; }
        public int InvReal { get; set; }
        public int categoryId { get; set; }
        public Category Category { get; set; }
    }
}
