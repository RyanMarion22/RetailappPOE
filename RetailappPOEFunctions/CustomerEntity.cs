using Azure;
using Azure.Data.Tables;
using System;

namespace RetailappPOEFunctions
{
    public class CustomerEntity : ITableEntity
    {
        // Make CustomerId a string to match MVC GUIDs
        public string CustomerId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        // ITableEntity implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
