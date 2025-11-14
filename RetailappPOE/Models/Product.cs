using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace RetailappPOE.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Products";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string ProductId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double Price { get; set; }

        public string ImageUrl { get; set; } = "";
    }
}
