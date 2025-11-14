using Azure;
using Azure.Data.Tables;

namespace RetailappPOE.Models
{
    public class Orders : ITableEntity
    {
        public string CustomerId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Newtonsoft.Json.JsonIgnore]
        public string? CustomerName { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string? ProductName { get; set; }

        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
