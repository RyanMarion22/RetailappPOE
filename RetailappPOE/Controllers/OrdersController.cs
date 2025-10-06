using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RetailAppPOE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "https://ryanst10440289part2-a3avddhxerhyhke4.southafricanorth-01.azurewebsites.net/api/";

        public OrdersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ==================== VIEW ALL ORDERS ====================
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_baseFunctionUrl + "orders");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load orders from Azure Function.";
                return View(new List<Orders>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<Orders>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Fetch customers and products for dropdown/display
            var customersResp = await _httpClient.GetAsync(_baseFunctionUrl + "customers");
            var productsResp = await _httpClient.GetAsync(_baseFunctionUrl + "products");

            ViewData["Customers"] = customersResp.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<List<Customers>>(await customersResp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new List<Customers>();

            ViewData["Products"] = productsResp.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<List<Product>>(await productsResp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new List<Product>();

            return View(orders);
        }

        // ==================== ADD ORDER (GET) ====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customersResp = await _httpClient.GetAsync(_baseFunctionUrl + "customers");
            var productsResp = await _httpClient.GetAsync(_baseFunctionUrl + "products");

            ViewData["Customers"] = customersResp.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<List<Customers>>(await customersResp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new List<Customers>();

            ViewData["Products"] = productsResp.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<List<Product>>(await productsResp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new List<Product>();

            return View(new Orders());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Orders order)
        {
            if (!ModelState.IsValid)
            {
                return await Create();
            }

            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseFunctionUrl + "orders", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Order added successfully.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to add order via Azure Function.");
            return await Create();
        }


        // ==================== DELETE ORDER ====================
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var deleteUrl = $"{_baseFunctionUrl}DeleteOrder?partitionKey={partitionKey}&rowKey={rowKey}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete order via Azure Function.";
            }
            else
            {
                TempData["Success"] = "Order deleted successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
