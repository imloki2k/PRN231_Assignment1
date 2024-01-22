using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using EstoreMVC.Models;

namespace EstoreAPI.Controllers
{
    public class OrdersController : Controller
    {
        private readonly EStoreContext _context;
        private readonly HttpClient _client = null;
        private string OrderApiUrl = "";

        public OrdersController(EStoreContext context)
        {
            _context = context;
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            OrderApiUrl = "http://localhost:5105/api/orders";
        }
        [BindProperty]
        public Order Orders { get; set; }
        [BindProperty]
        public OrderDetail OrderDetails { get; set; }
        // GET: Orders
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await _client.GetAsync(OrderApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
            List<Order> orders = JsonSerializer.Deserialize<List<Order>>(strData, options);
            return View(orders);
        }
        public async Task<IActionResult> Details(int? id)
        {
            string orderDetailApi = "http://localhost:5105/orderdetail/";
            HttpResponseMessage response = await _client.GetAsync(orderDetailApi + id);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
            List<OrderDetail> orders = JsonSerializer.Deserialize<List<OrderDetail>>(strData, options);
            ViewData["Order"] = orders;
            return View();
        }

        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,OrderDate,RequiredDate,ShippedDate,Freight,OrderDetails")] Order order)
        {
            
            var productId = int.Parse(Request.Form["ProductId"]);
            var discount = double.Parse(Request.Form["Discount"]);
            var quantity = int.Parse(Request.Form["Quantity"]);
            var orderDetal = new OrderDetail
            {
                Discount = (int)discount,
                Quantity = quantity,
                ProductId = productId
            };

            order.OrderDetails.Add(orderDetal);
            using (var respone = await _client.PostAsJsonAsync(OrderApiUrl, order))
            {
                string apiResponse = await respone.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", order.MemberId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,MemberId,OrderDate,RequiredDate,ShippedDate,Freight")] Order order)
        {

            using (var respone = await _client.PutAsJsonAsync(OrderApiUrl + "/" + id, order))
            {
                string apiResponse = await respone.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productId = _context.Orders.Find(id);
            if (productId != null)
            {
                String url = "http://localhost:5105/api/orders/" + id;
                await _client.DeleteAsync(url);
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
