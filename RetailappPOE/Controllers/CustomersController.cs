using RetailappPOE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace RetailappPOE.Controllers
{
    public class CustomersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "https://ryanst10440289part2-a3avddhxerhyhke4.southafricanorth-01.azurewebsites.net/api/";


        public CustomersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ==================== VIEW ALL CUSTOMERS ====================
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_baseFunctionUrl + "customers");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load customers from Azure Function.";
                return View(new List<Customers>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var customers = JsonSerializer.Deserialize<List<Customers>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(customers);
        }

        // ==================== ADD CUSTOMER (GET) ====================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Customers());
        }

        // ==================== ADD CUSTOMER (POST) ====================
        [HttpPost]
        public async Task<IActionResult> Create(Customers customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseFunctionUrl + "customers", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Customer added successfully.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to add customer via Azure Function.");
            return View(customer);
        }

        // ==================== DELETE CUSTOMER ====================
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var deleteUrl = $"{_baseFunctionUrl}DeleteCustomer?partitionKey={partitionKey}&rowKey={rowKey}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete customer via Azure Function.";
            }
            else
            {
                TempData["Success"] = "Customer deleted successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
