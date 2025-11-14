using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Data;
using RetailappPOE.Models;

namespace RetailappPOE.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public AdminController(ApplicationDbContext ctx) => _ctx = ctx;

        public IActionResult ManageOrders()
        {
            var orders = _ctx.Orders.ToList();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _ctx.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _ctx.SaveChanges();
            }
            return RedirectToAction(nameof(ManageOrders));
        }
    }
}
