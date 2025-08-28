using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using RetailappPOE.Services;

namespace RetailappPOE.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly BlobService _blobService;

        public ProductController(IConfiguration config, BlobService blobService)
        {
            var connectionString = config.GetConnectionString("AzureTableStorage")
                ?? throw new InvalidOperationException("AzureTableStorage connection string not found.");

            _tableService = new TableStorageService(connectionString, "Products");
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetProductsAsync();
            return View(products);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            if (imageFile != null)
            {
                using var stream = imageFile.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var imageUrl = await _blobService.UploadAsync(stream, fileName);
                product.ImageUrl = imageUrl;
            }

            await _tableService.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey, string imageUrl)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                await _blobService.DeleteBlobAsync(imageUrl);
            }

            await _tableService.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
