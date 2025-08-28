using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using RetailappPOE.Services;

namespace RetailappPOE.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(TableStorageService tableStorageService, ILogger<CustomersController> logger)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Display all customers
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _tableStorageService.GetCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers.");
                return View("Error", new { message = "An error occurred while retrieving customers." });
            }
        }

        // GET: Show form to create a new customer
        public IActionResult Create() => View(new Customers());

        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customers customer)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", customer);
            }

            try
            {
                customer.PartitionKey ??= "CUSTOMER";
                customer.RowKey ??= Guid.NewGuid().ToString();
                await _tableStorageService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving customer.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the customer.");
                return View("Create", customer);
            }
        }

        // GET: Delete a customer
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer.");
                return RedirectToAction(nameof(Index)); // Or show an error view
            }
        }
    }
}