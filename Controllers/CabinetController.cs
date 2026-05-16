using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jahongir_diplomIshi.Data;
using Jahongir_diplomIshi.Models;
using Jahongir_diplomIshi.Services;

namespace Jahongir_diplomIshi.Controllers
{
    [Authorize]
    public class CabinetController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly CalorieService _calorie;

        public CabinetController(AppDbContext db, UserManager<User> userManager, CalorieService calorie)
        {
            _db = db;
            _userManager = userManager;
            _calorie = calorie;
        }

        private async Task<User> GetCurrentUserAsync() =>
            (await _userManager.GetUserAsync(User))!;

        // GET /Cabinet
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            ViewBag.OrderCount     = await _db.Orders.CountAsync(o => o.UserId == user.Id);
            ViewBag.AddressCount   = await _db.Addresses.CountAsync(a => a.UserId == user.Id);
            ViewBag.FavoriteCount  = (user.FavoriteIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            return View(user);
        }

        // GET /Cabinet/Orders
        public async Task<IActionResult> Orders()
        {
            var user   = await GetCurrentUserAsync();
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Address)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        // GET /Cabinet/Favorites
        public async Task<IActionResult> Favorites()
        {
            var user = await GetCurrentUserAsync();
            var ids  = (user.FavoriteIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var v) ? v : 0)
                .Where(id => id > 0)
                .ToList();

            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            return View(products);
        }

        // POST /Cabinet/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var user = await GetCurrentUserAsync();
            var ids  = (user.FavoriteIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .ToList();

            bool added;
            if (ids.Contains(productId.ToString()))
            {
                ids.Remove(productId.ToString());
                added = false;
            }
            else
            {
                ids.Add(productId.ToString());
                added = true;
            }

            user.FavoriteIds = string.Join(",", ids);
            await _userManager.UpdateAsync(user);
            return Json(new { success = true, added });
        }

        // GET /Cabinet/Addresses
        public async Task<IActionResult> Addresses()
        {
            var user      = await GetCurrentUserAsync();
            var addresses = await _db.Addresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
            return View(addresses);
        }

        // POST /Cabinet/AddAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(Address model)
        {
            var user = await GetCurrentUserAsync();
            model.UserId = user.Id;

            // Birinchi manzil — default
            if (!await _db.Addresses.AnyAsync(a => a.UserId == user.Id))
                model.IsDefault = true;

            _db.Addresses.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Manzil qo'shildi.";
            return RedirectToAction(nameof(Addresses));
        }

        // POST /Cabinet/DeleteAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await GetCurrentUserAsync();
            var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (addr != null)
            {
                _db.Addresses.Remove(addr);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Manzil o'chirildi.";
            return RedirectToAction(nameof(Addresses));
        }

        // GET /Cabinet/AIAnalysis
        public async Task<IActionResult> AIAnalysis()
        {
            var user   = await GetCurrentUserAsync();
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            var result = _calorie.AnalyzeOrders(orders);
            return View(result);
        }

        // POST /Cabinet/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string? phone)
        {
            var user = await GetCurrentUserAsync();
            user.FullName = fullName;
            user.Phone    = phone;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Ma'lumotlar yangilandi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
