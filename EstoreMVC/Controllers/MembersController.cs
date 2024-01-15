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
using Microsoft.AspNetCore.Identity;

namespace EstoreMVC.Controllers
{
    public class MembersController : Controller
    {
        private readonly EStoreContext _context;
        private readonly HttpClient client;
        private string MemberUrl = "http://localhost:5105/api/Members";


        public MembersController(EStoreContext context)
        {
            _context = context;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            //return _context.Members != null ? 
            //            View(await _context.Members.ToListAsync()) :
            //            Problem("Entity set 'EStoreContext.Members'  is null.");

            try
            {
                HttpResponseMessage response = await client.GetAsync(MemberUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Member> members = JsonSerializer.Deserialize<List<Member>>(strData, options);
                return View(members);
            }
            catch
            {
                return NoContent();
            }

        }
        
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            string email, pass;
            var conf = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            email = conf.GetSection("Admin").GetSection("Email").Value.ToString();
            pass = conf.GetSection("Admin").GetSection("Password").Value.ToString();

            if(email == Email && pass == Password)
            {
                HttpContext.Session.SetInt32("Role", 1);
                HttpContext.Session.SetString("Email", email);
                return RedirectToAction("Index", "Home");
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(MemberUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Member> members = JsonSerializer.Deserialize<List<Member>>(strData, options);
                List<Member> users = members.Where(m => m.Email == Email
                    && m.Password == Password).ToList();
                if (users.Count == 0) return View();
                else
                {
                    HttpContext.Session.SetInt32("Role", 0);
                    HttpContext.Session.SetString("Email", users[0].Email);
                    HttpContext.Session.SetInt32("MemberId", users[0].MemberId);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                return View();
            }
            
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,Email,CompanyName,City,Country,Password")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,Email,CompanyName,City,Country,Password")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'EStoreContext.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
          return (_context.Members?.Any(e => e.MemberId == id)).GetValueOrDefault();
        }
    }
}
