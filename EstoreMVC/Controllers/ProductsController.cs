using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EstoreMVC.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EstoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EStoreContext _context;
        private readonly HttpClient client;
        private string ProductsUrl = "http://localhost:5105/api/Products";

        public ProductsController(EStoreContext context)
        {
            _context = context;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ProductsUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Product> products = JsonSerializer.Deserialize<List<Product>>(strData, options);
                return View(products);
            }
            catch
            {
                return NoContent();
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest(); // hoặc xử lý trường hợp id là null nếu cần
            }

            string productDetailApi = "http://localhost:5105/api/Products" + "/" + id;
            HttpResponseMessage response = await client.GetAsync(productDetailApi);

            if (response.IsSuccessStatusCode)
            {
                string strData = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                // Giải mã thành một đối tượng Product, không phải là một danh sách
                Product product = JsonSerializer.Deserialize<Product>(strData, options);

                //ViewData["Details"] = product; // Sử dụng "Product" thay vì "Products"
                return View(product);
            }
            else
            {
                // Xử lý trường hợp cuộc gọi API không thành công
                return StatusCode((int)response.StatusCode); // Bạn có thể điều chỉnh xử lý mã trạng thái tùy thuộc vào yêu cầu của ứng dụng
            }
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {
            using (var respone = await client.PostAsJsonAsync(ProductsUrl, product))
            {
                string apiResponse = await respone.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {
            if (!ModelState.IsValid)
            {
                
            }

            using (var response = await client.PutAsJsonAsync(ProductsUrl + "/" + id, product))
            {
                // Kiểm tra xem yêu cầu đã thành công hay không
                if (response.IsSuccessStatusCode)
                {
                    // Nếu thành công, chuyển hướng đến trang Index
                    return RedirectToAction("Index");
                }
                else
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productId = _context.Orders.Find(id);
            if (productId != null)
            {
                String url = "http://localhost:5105/api/Products/" + id;
                await client.DeleteAsync(url);
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
