using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RetailappPOE.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "https://ryanst10440289part2-a3avddhxerhyhke4.southafricanorth-01.azurewebsites.net/api/";

        public ProductController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ==================== VIEW ALL PRODUCTS ====================
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_baseFunctionUrl + "products");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load products from Azure Function.";
                return View(new List<Product>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(products);
        }

        // ==================== CREATE PRODUCT (GET) ====================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Product());
        }

        // ==================== CREATE PRODUCT (POST) ====================
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var json = JsonSerializer.Serialize(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseFunctionUrl + "products", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product added successfully.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to add product via Azure Function.");
            return View(product);
        }

        // ==================== DELETE PRODUCT ====================
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var deleteUrl = $"{_baseFunctionUrl}DeleteProduct?partitionKey={partitionKey}&rowKey={rowKey}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete product via Azure Function.";
            }
            else
            {
                TempData["Success"] = "Product deleted successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
