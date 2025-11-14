using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Data;
using RetailappPOE.Models;

namespace RetailappPOE.Controllers
{
    public class ProductSQLController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public ProductSQLController(ApplicationDbContext ctx) => _ctx = ctx;

        public IActionResult Index()
        {
            var products = _ctx.Products.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create() => View(new ProductSQL());

        [HttpPost]
        public IActionResult Create(ProductSQL product)
        {
            if (!ModelState.IsValid) return View(product);
            _ctx.Products.Add(product);
            _ctx.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _ctx.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(ProductSQL product)
        {
            if (!ModelState.IsValid) return View(product);
            _ctx.Products.Update(product);
            _ctx.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _ctx.Products.Find(id);
            if (product != null)
            {
                _ctx.Products.Remove(product);
                _ctx.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
