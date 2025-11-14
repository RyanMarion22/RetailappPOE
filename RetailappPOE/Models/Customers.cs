
using Azure;
using Azure.Data.Tables;

namespace RetailappPOE.Models
{
    public class Customers : ITableEntity
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Address { get; set; }
        public string? ContactNo { get; set; }

        public string Name => $"{FirstName} {LastName}".Trim();

        public string PartitionKey { get; set; } = "Customers";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}