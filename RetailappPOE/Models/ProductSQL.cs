using System.ComponentModel.DataAnnotations;

namespace RetailappPOE.Models
{
    public class ProductSQL
    {
        public int Id { get; set; }

        [Required, StringLength(250)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
    }
}
