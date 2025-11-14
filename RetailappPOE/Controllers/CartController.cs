using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailappPOE.Data;
using RetailappPOE.Models;
using RetailappPOE.Models.SQLModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RetailappPOE.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public CartController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            const int GUEST_ID = 1; 
            var order = _ctx.Orders
                .FirstOrDefault(o => o.CustomerId == GUEST_ID && o.Status == "PENDING");

            if (order == null)
            {
                order = new OrderSQL
                {
                    CustomerId = GUEST_ID,
                    Status = "PENDING",
                    OrderDate = DateTime.Now
                };
                _ctx.Orders.Add(order);
                _ctx.SaveChanges();
            }

            var existing = _ctx.CartItems
                .FirstOrDefault(c => c.OrderId == order.Id && c.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                _ctx.CartItems.Add(new CartItem
                {
                    OrderId = order.Id,
                    ProductId = productId,
                    Quantity = quantity
                });

            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            const int GUEST_ID = 1;
            var order = _ctx.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.CustomerId == GUEST_ID && o.Status == "PENDING");

            if (order == null || !order.Items.Any())
                return View(new List<CartItem>());

            ViewBag.OrderId = order.Id;
            ViewBag.Total = order.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0m));
            return View(order.Items);
        }

        [HttpPost]
        public IActionResult Checkout(int orderId)
        {
            var order = _ctx.Orders.Find(orderId);
            if (order == null) return NotFound();
            order.Status = "PLACED";
            order.OrderDate = DateTime.UtcNow;
            _ctx.SaveChanges();
            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation() => View();
    }
}
