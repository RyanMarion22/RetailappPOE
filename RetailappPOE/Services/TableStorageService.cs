using Azure;
using Azure.Data.Tables;
using RetailappPOE.Models;

namespace RetailappPOE.Services
{
    public class TableStorageService
    {
        private readonly TableClient _table;

        // Constructor can accept different table names (Products, Customers, Orders)
        public TableStorageService(string connectionString, string tableName)
        {
            _table = new TableClient(connectionString, tableName);
            _table.CreateIfNotExists();
        }

        // ---------------------- Product Methods ----------------------
        public async Task AddProductAsync(Product entity)
        {
            if (string.IsNullOrWhiteSpace(entity.PartitionKey))
                entity.PartitionKey = "PRODUCT";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            await _table.AddEntityAsync(entity);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var results = _table.Query<Product>();
            return await Task.FromResult(results.ToList());
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            await _table.DeleteEntityAsync(partitionKey, rowKey);
        }

        // ---------------------- Customer Methods ----------------------
        public async Task AddCustomerAsync(Customers entity)
        {
            if (string.IsNullOrWhiteSpace(entity.PartitionKey))
                entity.PartitionKey = "CUSTOMER";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            await _table.AddEntityAsync(entity);
        }

        public async Task<List<Customers>> GetCustomersAsync()
        {
            var results = _table.Query<Customers>();
            return await Task.FromResult(results.ToList());
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            await _table.DeleteEntityAsync(partitionKey, rowKey);
        }

        // ---------------------- Order Methods ----------------------
        public async Task AddOrderAsync(Orders entity)
        {
            if (string.IsNullOrWhiteSpace(entity.PartitionKey))
                entity.PartitionKey = "ORDER";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            await _table.AddEntityAsync(entity);
        }

        public async Task<List<Orders>> GetOrdersAsync()
        {
            var results = _table.Query<Orders>();
            return await Task.FromResult(results.ToList());
        }

        public async Task<Orders?> GetOrderByIdAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            try
            {
                var response = await _table.GetEntityAsync<Orders>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null; // not found
            }
        }

        public async Task UpdateOrderAsync(Orders entity)
        {
            if (string.IsNullOrWhiteSpace(entity.PartitionKey) || string.IsNullOrWhiteSpace(entity.RowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            await _table.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            await _table.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
