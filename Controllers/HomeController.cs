using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jahongir_diplomIshi.Data;
using Jahongir_diplomIshi.Models;
using Jahongir_diplomIshi.Services;
using Jahongir_diplomIshi.ViewModels;

namespace Jahongir_diplomIshi.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly SeasonalService _seasonal;

        public HomeController(AppDbContext db, SeasonalService seasonal)
        {
            _db = db;
            _seasonal = seasonal;
        }

        public async Task<IActionResult> Index(string? category, string? search, int page = 1)
        {
            var currentSeason = _seasonal.GetCurrentSeason();
            var seasonName    = _seasonal.GetCurrentSeasonName();
            var seasonEmoji   = _seasonal.GetCurrentSeasonEmoji();

            // Mavsumiy mahsulotlar (bosh sahifada)
            var seasonalProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Seasons != null && p.Seasons.Contains(currentSeason.ToString()))
                .OrderByDescending(p => p.IsFeatured)
                .Take(8)
                .ToListAsync();

            // Tanlangan mahsulotlar
            var featuredProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .Take(6)
                .ToListAsync();

            // Barcha kategoriyalar
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Qidiruv / filtrlash
            const int pageSize = 12;
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category!.Slug == category);

            var totalProducts = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentSeason    = currentSeason;
            ViewBag.SeasonName       = seasonName;
            ViewBag.SeasonEmoji      = seasonEmoji;
            ViewBag.SeasonalProducts = seasonalProducts;
            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.Categories       = categories;
            ViewBag.Search           = search;
            ViewBag.CurrentCategory  = category;
            ViewBag.TotalPages       = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.CurrentPage      = page;

            return View(products);
        }

        public async Task<IActionResult> Product(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            // O'xshash mahsulotlar
            var related = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            ViewBag.Related = related;
            return View(product);
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
