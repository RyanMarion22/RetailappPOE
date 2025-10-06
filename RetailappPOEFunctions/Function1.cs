using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using RetailappPOE.Models;

namespace RetailappPOEFunctions
{
    public class FunctionEndpoints
    {
        private readonly ILogger<FunctionEndpoints> _logger;
        private readonly string _storageConn;

        private const string CustomerTableName = "Customers";
        private const string ProductTableName = "Products";
        private const string OrderTableName = "Orders";
        private const string ImageContainerName = "images";
        private const string UploadShareName = "uploads";

        public FunctionEndpoints(ILogger<FunctionEndpoints> logger)
        {
            _logger = logger;
            _storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                           ?? throw new InvalidOperationException("AzureWebJobsStorage environment variable is not set.");
        }

        // ========================= QUEUE TRIGGERS =========================

        [Function("QueueCustomerSender")]
        public async Task QueueCustomerSender(
            [QueueTrigger("customer-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            FunctionContext ctx)
        {
            _logger.LogInformation("Processing customer queue message.");

            try
            {
                var table = new TableClient(_storageConn, CustomerTableName);
                await table.CreateIfNotExistsAsync();

                var customer = JsonSerializer.Deserialize<CustomerEntity>(queueMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (customer == null)
                {
                    _logger.LogError("Failed to deserialize customer JSON");
                    return;
                }

                customer.RowKey = Guid.NewGuid().ToString();
                customer.PartitionKey = "Customers";

                await table.AddEntityAsync(customer);
                _logger.LogInformation("Saved customer {Name} to Table Storage", customer.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueueCustomerSender failed");
                throw;
            }
        }

        [Function("QueueProductSender")]
        public async Task QueueProductSender(
            [QueueTrigger("product-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            FunctionContext ctx)
        {
            _logger.LogInformation("Processing product queue message.");

            try
            {
                var table = new TableClient(_storageConn, ProductTableName);
                await table.CreateIfNotExistsAsync();

                var product = JsonSerializer.Deserialize<ProductEntity>(queueMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (product == null)
                {
                    _logger.LogError("Failed to deserialize product JSON");
                    return;
                }

                product.RowKey = Guid.NewGuid().ToString();
                product.PartitionKey = "Products";

                await table.AddEntityAsync(product);
                _logger.LogInformation("Saved product {Name} to Table Storage", product.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueueProductSender failed");
                throw;
            }
        }

        [Function("QueueOrderSender")]
        public async Task QueueOrderSender(
            [QueueTrigger("order-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            FunctionContext ctx)
        {
            _logger.LogInformation("Processing order queue message.");

            try
            {
                var orderDto = JsonSerializer.Deserialize<Orders>(queueMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (orderDto == null)
                {
                    _logger.LogError("Failed to deserialize order JSON");
                    return;
                }

                var table = new TableClient(_storageConn, OrderTableName);
                await table.CreateIfNotExistsAsync();

                var orderEntity = new Orders
                {
                    PartitionKey = "Orders",
                    RowKey = Guid.NewGuid().ToString(),
                    CustomerId = orderDto.CustomerId,
                    ProductId = orderDto.ProductId,
                    Quantity = orderDto.Quantity,
                    OrderDate = orderDto.OrderDate
                };

                await table.AddEntityAsync(orderEntity);
                _logger.LogInformation("Saved order {RowKey} to Table Storage", orderEntity.RowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueueOrderSender failed");
                throw;
            }
        }

        // ========================= GET ENDPOINTS =========================

        [Function("GetCustomers")]
        public async Task<HttpResponseData> GetCustomers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequestData req)
        {
            try
            {
                var table = new TableClient(_storageConn, CustomerTableName);
                await table.CreateIfNotExistsAsync();

                var results = new List<CustomerEntity>();
                await foreach (var e in table.QueryAsync<CustomerEntity>())
                    results.Add(e);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(results);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query customers");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Error retrieving customers.");
                return error;
            }
        }

        [Function("GetProducts")]
        public async Task<HttpResponseData> GetProducts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
        {
            try
            {
                var table = new TableClient(_storageConn, ProductTableName);
                await table.CreateIfNotExistsAsync();

                var results = new List<ProductEntity>();
                await foreach (var e in table.QueryAsync<ProductEntity>())
                    results.Add(e);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(results);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query products");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Error retrieving products.");
                return error;
            }
        }

        // ========================= ADD / DELETE PRODUCT =========================

        [Function("AddProduct")]
        public async Task<HttpResponseData> AddProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonSerializer.Deserialize<ProductEntity>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null || string.IsNullOrEmpty(product.Name))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid product data");
                return bad;
            }

            product.PartitionKey = "Products";
            product.RowKey = Guid.NewGuid().ToString();

            var table = new TableClient(_storageConn, ProductTableName);
            await table.CreateIfNotExistsAsync();
            await table.AddEntityAsync(product);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("Product added successfully");
            return response;
        }

        [Function("DeleteProduct")]
        public async Task<HttpResponseData> DeleteProduct([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{partitionKey}/{rowKey}")] HttpRequestData req, string partitionKey, string rowKey)
        {
            try
            {
                var table = new TableClient(_storageConn, ProductTableName);
                await table.DeleteEntityAsync(partitionKey, rowKey);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteStringAsync("Product deleted successfully");
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to delete product.");
                return error;
            }
        }

        // ========================= ADD CUSTOMER =========================

        [Function("AddCustomer")]
        public async Task<HttpResponseData> AddCustomer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newCustomer = JsonSerializer.Deserialize<CustomerEntity>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (newCustomer == null || string.IsNullOrEmpty(newCustomer.Name) || string.IsNullOrEmpty(newCustomer.Email))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid customer data. Name and Email are required.");
                return badResponse;
            }

            newCustomer.PartitionKey = "Customers";
            newCustomer.RowKey = Guid.NewGuid().ToString();

            try
            {
                var table = new TableClient(_storageConn, CustomerTableName);
                await table.CreateIfNotExistsAsync();
                await table.AddEntityAsync(newCustomer);

                var queueClient = new QueueClient(_storageConn, "customer-queue");
                await queueClient.CreateIfNotExistsAsync();
                string json = JsonSerializer.Serialize(newCustomer);
                await queueClient.SendMessageAsync(json);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync("Customer added successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add customer");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to add customer.");
                return error;
            }
        }

        // ========================= ADD ORDER =========================

        [Function("AddOrder")]
        public async Task<HttpResponseData> AddOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var newOrder = JsonSerializer.Deserialize<Orders>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (newOrder == null || string.IsNullOrEmpty(newOrder.CustomerId) || string.IsNullOrEmpty(newOrder.ProductId) || newOrder.Quantity <= 0)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid order data. Make sure CustomerId, ProductId, and Quantity are provided.");
                return badResponse;
            }

            newOrder.PartitionKey = "Orders";
            newOrder.RowKey = Guid.NewGuid().ToString();
            newOrder.Timestamp = DateTimeOffset.UtcNow;
            newOrder.OrderDate = DateTime.SpecifyKind(newOrder.OrderDate, DateTimeKind.Utc);

            try
            {
                var table = new TableClient(_storageConn, OrderTableName);
                await table.CreateIfNotExistsAsync();
                await table.AddEntityAsync(newOrder);

                var queueClient = new QueueClient(_storageConn, "order-queue");
                await queueClient.CreateIfNotExistsAsync();

                string json = JsonSerializer.Serialize(newOrder);
                await queueClient.SendMessageAsync(json);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync("Order added successfully and queued.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add order");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Failed to add order: {ex.Message}");
                return errorResponse;
            }
        }

        // ========================= FILE UPLOADS =========================

        [Function("UploadToAzureFiles")]
        public async Task<HttpResponseData> UploadToAzureFiles([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "uploads")] HttpRequestData req)
        {
            try
            {
                if (!req.Headers.TryGetValues("Content-Type", out var ct) || string.IsNullOrEmpty(ct.FirstOrDefault()))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("Content-Type header missing");
                    return bad;
                }

                var contentType = ct.First();
                if (!MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("Invalid Content-Type");
                    return bad;
                }

                var boundary = HeaderUtilities.RemoveQuotes(parsedContentType.Boundary).Value;
                if (string.IsNullOrEmpty(boundary))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("Missing multipart boundary");
                    return bad;
                }

                var reader = new MultipartReader(boundary, req.Body);
                var section = await reader.ReadNextSectionAsync();
                if (section == null)
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("No multipart data");
                    return bad;
                }

                while (section != null)
                {
                    if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var cd) &&
                        cd.DispositionType.Equals("form-data") &&
                        !string.IsNullOrEmpty(cd.FileName.Value))
                    {
                        var fileName = cd.FileName.Value.Trim('"');
                        using var ms = new MemoryStream();
                        await section.Body.CopyToAsync(ms);
                        ms.Position = 0;

                        var share = new ShareClient(_storageConn, UploadShareName);
                        await share.CreateIfNotExistsAsync();

                        var root = share.GetRootDirectoryClient();
                        var fileClient = root.GetFileClient(fileName);

                        await fileClient.CreateAsync(ms.Length);
                        ms.Position = 0;
                        await fileClient.UploadRangeAsync(new Azure.HttpRange(0, ms.Length), ms);

                        var ok = req.CreateResponse(HttpStatusCode.OK);
                        await ok.WriteStringAsync($"File '{fileName}' uploaded successfully!");
                        return ok;
                    }

                    section = await reader.ReadNextSectionAsync();
                }

                var notFound = req.CreateResponse(HttpStatusCode.BadRequest);
                await notFound.WriteStringAsync("No file found in multipart content");
                return notFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Upload failed: {ex.Message}");
                return error;
            }
        }

        [Function("GetUploadedFiles")]
        public async Task<HttpResponseData> GetUploadedFiles([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "uploads")] HttpRequestData req)
        {
            try
            {
                var files = new List<FileEntity>();
                var share = new ShareClient(_storageConn, UploadShareName);
                await share.CreateIfNotExistsAsync();

                var root = share.GetRootDirectoryClient();
                await foreach (var item in root.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = root.GetFileClient(item.Name);
                        var props = await fileClient.GetPropertiesAsync();

                        files.Add(new FileEntity
                        {
                            FileName = item.Name,
                            Size = props.Value.ContentLength,
                            DisplaySize = FormatSize(props.Value.ContentLength),
                            LastModified = props.Value.LastModified
                        });
                    }
                }

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(files);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files");
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Failed to list files: {ex.Message}");
                return err;
            }
        }

        [Function("UploadProductImage")]
        public async Task<HttpResponseData> UploadProductImage([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blobs/upload")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("{\"error\":\"empty body\"}");
                return bad;
            }

            FileUploadDto payload;
            try
            {
                payload = JsonSerializer.Deserialize<FileUploadDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (payload == null || string.IsNullOrEmpty(payload.FileName) || string.IsNullOrEmpty(payload.Base64))
                    throw new Exception("missing fields");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid payload for blob upload");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("{\"error\":\"invalid payload (need fileName + base64)\"}");
                return bad;
            }

            byte[] bytes;
            try { bytes = Convert.FromBase64String(payload.Base64); }
            catch
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("{\"error\":\"base64 invalid\"}");
                return bad;
            }

            try
            {
                var blobService = new BlobServiceClient(_storageConn);
                var container = blobService.GetBlobContainerClient(ImageContainerName);
                await container.CreateIfNotExistsAsync();

                string unique = $"{Guid.NewGuid()}_{Path.GetFileName(payload.FileName)}";
                var client = container.GetBlobClient(unique);
                using var ms = new MemoryStream(bytes);
                await client.UploadAsync(ms, overwrite: true);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync(JsonSerializer.Serialize(new { url = client.Uri.ToString() }));
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob upload failed");
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync("{\"error\":\"blob upload failed\"}");
                return err;
            }
        }

        // Helper DTO
        private class FileUploadDto
        {
            public string FileName { get; set; }
            public string Base64 { get; set; }
        }

        // Helper method
        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024 * 1024.0):F2} MB";
            else if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            else
                return $"{bytes} B";
        }
    }
}