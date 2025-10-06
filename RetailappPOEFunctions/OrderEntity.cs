using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace RetailappPOE.Models
{
    public class Orders : ITableEntity
    {
        [Key]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Please select a customer.")]
        public string CustomerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a product.")]
        public string ProductId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a quantity.")]
        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Azure Table Storage fields
        public string PartitionKey { get; set; } = "OrderPartition";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
