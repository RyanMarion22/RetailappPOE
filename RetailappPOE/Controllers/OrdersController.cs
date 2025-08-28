using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using RetailappPOE.Services;

namespace RetailappPOE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _orderService;
        private readonly TableStorageService _customerService;
        private readonly TableStorageService _productService;
        private readonly QueueService _queueService;

        public OrdersController(IConfiguration configuration, QueueService queueService)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("AzureStorage connection string not found.");

            _orderService = new TableStorageService(connectionString, "Orders");
            _customerService = new TableStorageService(connectionString, "Customers");
            _productService = new TableStorageService(connectionString, "Products");
            _queueService = queueService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetOrdersAsync();
            var customers = await _customerService.GetCustomersAsync();
            var products = await _productService.GetProductsAsync();

            ViewData["Customers"] = customers;
            ViewData["Products"] = products;

            return View(orders);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Customers"] = await _customerService.GetCustomersAsync();
            ViewData["Products"] = await _productService.GetProductsAsync();
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Orders order)
        {
            if (ModelState.IsValid)
            {
                order.PartitionKey = "ORDER";
                order.RowKey = Guid.NewGuid().ToString();
                order.OrderDate = DateTime.UtcNow;

                await _orderService.AddOrderAsync(order);

                // Send order message to queue
                await _queueService.SendMessageAsync(order);

                return RedirectToAction(nameof(Index));
            }

            ViewData["Customers"] = await _customerService.GetCustomersAsync();
            ViewData["Products"] = await _productService.GetProductsAsync();
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var order = await _orderService.GetOrderByIdAsync(partitionKey, rowKey);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Orders order)
        {
            await _orderService.DeleteOrderAsync(order.PartitionKey, order.RowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
