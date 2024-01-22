using EstoreMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EstoreMVC.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly EStoreContext _context;
        private string _url = "http://localhost:5105/api/orderdetails";
        private readonly HttpClient _client = null;
        public OrderDetailController(EStoreContext context)
        {

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage responseMessage = await _client.GetAsync(_url);
            var data = await responseMessage.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };


            return View();
        }

        [BindProperty]
        public List<OrderDetail> data_list { get; set; }
        public async Task<IActionResult> get_detail(int? id)
        {
            if (id == null || id <= -1)
            {
                return NotFound();
            }
            else
            {
                HttpResponseMessage resp = await _client.GetAsync(_url + $"/{id}");
                var data = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                data_list = JsonSerializer.Deserialize<List<OrderDetail>>(data, options);
            }
            return View(data_list);
        }

        [HttpGet]
        public IActionResult CreateOrderDetail()
        {
            return View();
        }

        public async Task<IActionResult> DeleteDetail(int? id)
        {

            if (id == null || id <= -1)
            {
                return NotFound();
            }
            else
            {

                HttpResponseMessage resp = await _client.GetAsync(_url + $"/{id}");
            }
            return RedirectToAction("Index");
        }
    }
}
