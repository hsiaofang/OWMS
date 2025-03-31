using Microsoft.AspNetCore.Mvc;
using OWMS.Data;
using OWMS.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing.Imaging;

namespace OWMS.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }
                    product.PhotoUrl = $"/images/{fileName}";
                }
                product.QRCode = GenerateQRCodeBase64($"https://yourdomain.com/products/{product.Id}");
                product.CreatedAt = DateTime.UtcNow;

                _context.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch");
            }

            if (ModelState.IsValid)
            {
                // Handle image file upload
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }
                    product.PhotoUrl = $"/images/{fileName}";
                }

                // Generate QR Code based on the product's ID or a meaningful string
                product.QRCode = GenerateQRCodeBase64($"https://yourdomain.com/products/{product.Id}");
                _context.Update(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("qrcode/{id}")]
        public IActionResult GetQRCode(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var qrCodeData = GenerateQRCodeBase64($"https://localhost:5000/products/{product.Id}");
            return Ok(new { qrCodeBase64 = qrCodeData });
        }

        private string GenerateQRCodeBase64(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrCodeData))
                {
                    using (var ms = new MemoryStream())
                    {
                        qrCode.GetGraphic(20).Save(ms, ImageFormat.Png);
                        byte[] byteArray = ms.ToArray();
                        return Convert.ToBase64String(byteArray);
                    }
                }
            }
        }
    }
}