//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using OWMS.Data;
//using OWMS.Models;

//namespace OWMS.Controllers
//{
//    [Route("api/vendor")]
//    [ApiController]
//    public class VendorController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public VendorController(ApplicationDbContext context)
//        {
//            _context = context;
//        }
        
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors()
//        {
//            return await _context.Vendors.ToListAsync();
//        }

//        [HttpPost]
//        public async Task<ActionResult<Vendor>> PostVendor(Vendor vendor)
//        {
//            _context.Vendors.Add(vendor);
//            await _context.SaveChangesAsync();

//            //return CreatedAtAction(nameof(GetVendor), new { id = vendor.VendorId }, vendor);
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteVendor(int id)
//        {
//            var vendor = await _context.Vendors.FindAsync(id);
//            if (vendor == null)
//            {
//                return NotFound();
//            }

//            _context.Vendors.Remove(vendor);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }


//        // 搜尋廠商，根據廠商名稱、聯絡人
//        [HttpGet("search")]
//        public async Task<ActionResult<IEnumerable<Vendor>>> SearchVendors(
//            [FromQuery] string vendorName,
//            [FromQuery] string contact)
//        {
//            IQueryable<Vendor> query = _context.Vendors;

//            if (!string.IsNullOrEmpty(vendorName))
//            {
//                query = query.Where(v => v.VendorName.Contains(vendorName));
//            }

//            if (!string.IsNullOrEmpty(contact))
//            {
//                query = query.Where(v => v.Contact.Contains(contactPerson));
//            }

//            var vendors = await query.ToListAsync();

//            return Ok(vendors);
//        }



//    }
//}
