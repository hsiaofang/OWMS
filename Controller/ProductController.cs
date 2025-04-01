using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using OWMS.Data;
using OWMS.Models;
using QRCoder;

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

        // 新增
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (ModelState.IsValid)
            {
                // 处理图片上传
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    product.PhotoUrl = await SaveProductImage(product.ImageFile);
                }

                // 生成二维码
                product.QRCode = GenerateQRCode(product.ProductName);
                product.CreatedAt = DateTime.UtcNow;

                _context.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAllProducts), new { id = product.Id }, product);
            }
            return BadRequest(ModelState);
        }

        // 修改
        [HttpPut("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products
                                        .Include(p => p.Vendor)
                                        .Include(p => p.Counter)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // 更新产品信息
            product.ProductName = updatedProduct.ProductName;
            product.Price = updatedProduct.Price;
            product.Notes = updatedProduct.Notes;

            // 更新关联的 Vendor 和 Counter
            product.VendorId = updatedProduct.VendorId;
            product.CounterId = updatedProduct.CounterId;

            // 生成新的二维码
            product.QRCode = GenerateQRCode(updatedProduct.ProductName);

            // 如果有新的图片，处理图片上传
            if (updatedProduct.ImageFile != null && updatedProduct.ImageFile.Length > 0)
            {
                product.PhotoUrl = await SaveProductImage(updatedProduct.ImageFile);
            }

            // 更新产品信息
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // 刪除
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // 查找產品
            var product = await _context.Products.FindAsync(id);


            // 刪除產品
            _context.Products.Remove(product);

            // 保存更改
            await _context.SaveChangesAsync();

            // 返回204 No Content，表示成功刪除
            return NoContent();
        }


        private async Task<string> SaveProductImage(IFormFile imageFile)
        {
            // 生成文件名
            var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", newFileName);

            // 保存文件
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(file);
            }

            // 返回文件的URL
            return $"/images/{newFileName}";
        }

        private string GenerateQRCode(string data)
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
