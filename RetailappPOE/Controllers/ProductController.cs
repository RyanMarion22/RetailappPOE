using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Data;
using RetailappPOE.Models;
using RetailappPOE.Models.SQLModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RetailappPOE.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProductController(HttpClient httpClient, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _baseFunctionUrl = config["FunctionApi:BaseUrl"] ?? "http://localhost:7274/api/";
            _scopeFactory = scopeFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync(_baseFunctionUrl + "products");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to load products from Azure Function.";
                    return View(new List<Product>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiProducts = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                // SYNC TO SQL
                using var scope = _scopeFactory.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var api in apiProducts)
                {
                    if (!int.TryParse(api.RowKey.Split('-').LastOrDefault(), out int sqlId)) continue;
                    if (!ctx.Products.Any(p => p.Id == sqlId))
                    {
                        ctx.Products.Add(new ProductSQL
                        {
                            Id = sqlId,
                            Name = api.Name ?? "Unknown",
                            Description = api.Description,
                            Price = (decimal)api.Price,
                            ImageUrl = api.ImageUrl
                        });
                    }
                }
                await ctx.SaveChangesAsync();

                return View(apiProducts);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "Could not reach API: " + ex.Message;
                return View(new List<Product>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(new List<Product>());
            }
        }

        [HttpGet]
        public IActionResult Create() => View(new Product());

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);
            try
            {
                var json = JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_baseFunctionUrl + "products", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Product added successfully.";
                    return RedirectToAction("Index");
                }
                var body = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API Error: {response.StatusCode}. {body}");
                return View(product);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey, string imageUrl)
        {
            try
            {
                var deleteUrl = $"{_baseFunctionUrl}products/{partitionKey}/{rowKey}";
                var response = await _httpClient.DeleteAsync(deleteUrl);
                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Product deleted.";
                else
                    TempData["Error"] = $"Failed: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
