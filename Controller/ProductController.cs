using Microsoft.AspNetCore.Mvc;
using OWMS.Data;
using OWMS.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OWMS.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // 顯示新增頁面
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 新增
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.ImageFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }
                    product.PhotoUrl = $"/images/{product.ImageFile.FileName}";
                }

                product.QRCode = GenerateQRCode(product.ProductName);

                product.CreatedAt = DateTime.UtcNow;

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 顯示編輯頁面
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // 編輯
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.ImageFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }
                    product.PhotoUrl = $"/images/{product.ImageFile.FileName}";
                }

                product.QRCode = GenerateQRCode(product.ProductName);

                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 顯示刪除頁面
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 刪除
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private string GenerateQRCode(string data)
        {
            return $"QRCode_for_{data}";
        }
    }
}
