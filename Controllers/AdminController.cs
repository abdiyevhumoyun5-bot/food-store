using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jahongir_diplomIshi.Data;
using Jahongir_diplomIshi.Models;
using Jahongir_diplomIshi.Services;
using Jahongir_diplomIshi.ViewModels;

namespace Jahongir_diplomIshi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly FileUploadService _fileUpload;

        public AdminController(AppDbContext db, UserManager<User> userManager, FileUploadService fileUpload)
        {
            _db = db;
            _userManager = userManager;
            _fileUpload = fileUpload;
        }

        // GET /admin
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _db.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalOrders   = await _db.Orders.CountAsync();
            ViewBag.TotalUsers    = await _db.Users.CountAsync();
            ViewBag.TotalRevenue  = await _db.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.PendingOrders = await _db.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);

            ViewBag.RecentOrders  = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // ===================== MAHSULOTLAR =====================

        // GET /admin/products
        public async Task<IActionResult> Products(int page = 1, string? search = null)
        {
            const int pageSize = 15;
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            var total    = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages   = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage  = page;
            ViewBag.Search       = search;
            ViewBag.Categories   = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(products);
        }

        // GET /admin/product-form
        [HttpGet]
        public async Task<IActionResult> ProductForm(int? id)
        {
            var vm = new ProductCreateViewModel
            {
                Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
            };

            if (id.HasValue)
            {
                var p = await _db.Products.FindAsync(id.Value);
                if (p == null) return NotFound();
                vm.Name             = p.Name;
                vm.Description      = p.Description;
                vm.Price            = p.Price;
                vm.Unit             = p.Unit;
                vm.Stock            = p.Stock;
                vm.ImageUrl         = p.ImageUrl;
                vm.IsFeatured       = p.IsFeatured;
                vm.CategoryId       = p.CategoryId;
                vm.CaloriesPer100g  = p.CaloriesPer100g;
                vm.SelectedSeasons  = p.Seasons?.Split(',').Select(s => s.Trim()).ToList() ?? new();
                ViewBag.EditId = id;
            }

            return View(vm);
        }

        // POST /admin/product-form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductForm(int? id, ProductCreateViewModel vm)
        {
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            if (!ModelState.IsValid) { ViewBag.EditId = id; return View(vm); }

            string? imagePath = null;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                try { imagePath = await _fileUpload.SaveImageAsync(vm.ImageFile); }
                catch (Exception ex) { ModelState.AddModelError("ImageFile", ex.Message); ViewBag.EditId = id; return View(vm); }
            }

            if (id.HasValue)
            {
                var product = await _db.Products.FindAsync(id.Value);
                if (product == null) return NotFound();

                product.Name            = vm.Name;
                product.Description     = vm.Description;
                product.Price           = vm.Price;
                product.Unit            = vm.Unit;
                product.Stock           = vm.Stock;
                product.IsFeatured      = vm.IsFeatured;
                product.CategoryId      = vm.CategoryId;
                product.CaloriesPer100g = vm.CaloriesPer100g;
                product.Seasons         = vm.SelectedSeasons.Count > 0 ? string.Join(",", vm.SelectedSeasons) : null;
                product.UpdatedAt       = DateTime.UtcNow;

                if (imagePath != null)
                {
                    _fileUpload.DeleteImage(product.ImagePath);
                    product.ImagePath = imagePath;
                    product.ImageUrl  = null;
                }
                else if (!string.IsNullOrEmpty(vm.ImageUrl))
                {
                    product.ImageUrl  = vm.ImageUrl;
                    product.ImagePath = null;
                }
            }
            else
            {
                var product = new Product
                {
                    Name            = vm.Name,
                    Description     = vm.Description,
                    Price           = vm.Price,
                    Unit            = vm.Unit,
                    Stock           = vm.Stock,
                    IsFeatured      = vm.IsFeatured,
                    CategoryId      = vm.CategoryId,
                    CaloriesPer100g = vm.CaloriesPer100g,
                    Seasons         = vm.SelectedSeasons.Count > 0 ? string.Join(",", vm.SelectedSeasons) : null,
                    ImagePath       = imagePath,
                    ImageUrl        = imagePath == null ? vm.ImageUrl : null,
                };
                _db.Products.Add(product);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = id.HasValue ? "Mahsulot yangilandi." : "Mahsulot qo'shildi.";
            return RedirectToAction(nameof(Products));
        }

        // POST /admin/delete-product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive  = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Mahsulot o'chirildi.";
            return RedirectToAction(nameof(Products));
        }

        // ===================== BUYURTMALAR =====================

        // GET /admin/orders
        public async Task<IActionResult> Orders(string? status = null)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .Include(o => o.Address)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var s))
                query = query.Where(o => o.Status == s);

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        // POST /admin/update-order-status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status, string? paymentStatus)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order != null)
            {
                if (Enum.TryParse<OrderStatus>(status, out var os))
                    order.Status = os;
                if (!string.IsNullOrEmpty(paymentStatus) && Enum.TryParse<PaymentStatus>(paymentStatus, out var ps))
                    order.PaymentStatus = ps;
                order.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Orders));
        }

        // ===================== FOYDALANUVCHILAR =====================

        // GET /admin/users
        public async Task<IActionResult> Users(string? search = null, int page = 1)
        {
            const int pageSize = 20;
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.ToLower().Contains(search.ToLower()) ||
                                         (u.Email != null && u.Email.ToLower().Contains(search.ToLower())));

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search      = search;
            ViewBag.Total       = total;
            return View(users);
        }

        // POST /admin/toggle-user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
