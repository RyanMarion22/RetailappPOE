
using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace RetailappPOE.Models
{
    public class CustomerEntity : ITableEntity
    {
        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required] [EmailAddress] public string Email { get; set; } = "";
        public string? Address { get; set; }
        public string? ContactNo { get; set; }

        // Computed for dropdown
        public string Name => $"{FirstName} {LastName}".Trim();

        // ITableEntity
        public string PartitionKey { get; set; } = "Customers";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}