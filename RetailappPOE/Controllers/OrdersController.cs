using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RetailappPOE.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RetailappPOE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "http://localhost:7274/api/";

        public OrdersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var orders = new List<Orders>();
            var customers = new List<Customers>();
            var products = new List<Product>();

            try
            {
                // Fetch Orders
                var orderResp = await _httpClient.GetAsync(_baseFunctionUrl + "orders");
                if (orderResp.IsSuccessStatusCode)
                {
                    var json = await orderResp.Content.ReadAsStringAsync();
                    orders = JsonSerializer.Deserialize<List<Orders>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }

                // Fetch Customers
                var custResp = await _httpClient.GetAsync(_baseFunctionUrl + "customers");
                if (custResp.IsSuccessStatusCode)
                {
                    var json = await custResp.Content.ReadAsStringAsync();
                    customers = JsonSerializer.Deserialize<List<Customers>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }

                // Fetch Products
                var prodResp = await _httpClient.GetAsync(_baseFunctionUrl + "products");
                if (prodResp.IsSuccessStatusCode)
                {
                    var json = await prodResp.Content.ReadAsStringAsync();
                    products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load data: " + ex.Message;
            }

            // Enrich orders with names
            foreach (var order in orders)
            {
                order.CustomerName = customers.FirstOrDefault(c => c.RowKey == order.CustomerId)?.Name ?? "Unknown";
                order.ProductName = products.FirstOrDefault(p => p.RowKey == order.ProductId)?.Name ?? "Unknown";
            }

            ViewBag.CustomerList = customers;
            ViewBag.ProductList = products;

            ViewData["Customers"] = new SelectList(
                customers.Select(c => new { c.RowKey, c.Name }),
                "RowKey", "Name");

            ViewData["Products"] = new SelectList(
                products.Select(p => new { p.RowKey, p.Name }),
                "RowKey", "Name");

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new Orders());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Orders order)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(order);
            }

            order.RowKey ??= Guid.NewGuid().ToString();
            order.PartitionKey = "Orders";
            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_baseFunctionUrl + "orders", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Order added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Failed to save: " + error);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Connection error: " + ex.Message);
            }

            await LoadDropdowns();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var url = $"{_baseFunctionUrl}orders/{partitionKey}/{rowKey}";
                var response = await _httpClient.DeleteAsync(url);
                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Order deleted.";
                else
                    TempData["Error"] = "Failed to delete: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Connection error: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            var customers = new List<Customers>();
            var products = new List<Product>();

            try
            {
                var custResp = await _httpClient.GetAsync(_baseFunctionUrl + "customers");
                if (custResp.IsSuccessStatusCode)
                {
                    var json = await custResp.Content.ReadAsStringAsync();
                    customers = JsonSerializer.Deserialize<List<Customers>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch { }

            try
            {
                var prodResp = await _httpClient.GetAsync(_baseFunctionUrl + "products");
                if (prodResp.IsSuccessStatusCode)
                {
                    var json = await prodResp.Content.ReadAsStringAsync();
                    products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch { }

            ViewData["Customers"] = new SelectList(
                customers.Select(c => new { c.RowKey, c.Name }),
                "RowKey", "Name");

            ViewData["Products"] = new SelectList(
                products.Select(p => new { p.RowKey, p.Name }),
                "RowKey", "Name");
        }
    }
}