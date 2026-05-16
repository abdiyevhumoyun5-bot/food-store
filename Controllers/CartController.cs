using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Jahongir_diplomIshi.Data;
using Jahongir_diplomIshi.Models;
using Jahongir_diplomIshi.ViewModels;

namespace Jahongir_diplomIshi.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "cart";
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public CartController(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Session dan savatni o'qish
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        // Session ga savatni yozish
        private void SaveCart(List<CartItem> items)
        {
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(items));
        }

        // GET /Cart
        public async Task<IActionResult> Index()
        {
            var items = GetCart();
            var vm = new CartViewModel { Items = items };

            var checkout = new CheckoutViewModel
            {
                Items      = items,
                Subtotal   = items.Sum(i => i.Price * i.Quantity),
                DeliveryFee = 15000,
                Total      = items.Sum(i => i.Price * i.Quantity) + 15000,
            };

            // Login bo'lgan foydalanuvchining manzillari
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    checkout.SavedAddresses = await _db.Addresses
                        .Where(a => a.UserId == user.Id)
                        .OrderByDescending(a => a.IsDefault)
                        .ToListAsync();
                }
            }

            ViewBag.Cart = vm;
            return View(checkout);
        }

        // POST /Cart/Add
        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == productId && p.IsActive);
            if (product == null) return Json(new { success = false, message = "Mahsulot topilmadi" });

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name      = product.Name,
                    Price     = product.Price,
                    Quantity  = quantity,
                    Unit      = product.Unit,
                    Image     = product.DisplayImage,
                });

            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(i => i.Quantity) });
        }

        // POST /Cart/Update
        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else item.Quantity = quantity;
            }
            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(i => i.Quantity) });
        }

        // POST /Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(i => i.Quantity) });
        }

        // GET /Cart/Count
        public IActionResult Count()
        {
            var cart  = GetCart();
            return Json(new { count = cart.Sum(i => i.Quantity) });
        }

        // GET /Cart/Items
        public IActionResult Items()
        {
            var cart = GetCart();
            return Json(cart);
        }

        // POST /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cartItems = GetCart();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Savat bo'sh.";
                return RedirectToAction(nameof(Index));
            }

            // Yetkazib berish uchun manzil talab qilinadi
            if (model.OrderType == "delivery")
            {
                if (!model.SelectedAddressId.HasValue &&
                    (string.IsNullOrEmpty(model.City) || string.IsNullOrEmpty(model.Street) || string.IsNullOrEmpty(model.DeliveryPhone)))
                {
                    TempData["Error"] = "Yetkazib berish manzilini to'ldiring.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Mehmon uchun ism va telefon talab qilinadi
            if (!User.Identity!.IsAuthenticated &&
                (string.IsNullOrEmpty(model.GuestName) || string.IsNullOrEmpty(model.GuestPhone)))
            {
                TempData["Error"] = "Ism va telefon raqamini kiriting.";
                return RedirectToAction(nameof(Index));
            }

            User? user = null;
            int? addressId = null;

            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.GetUserAsync(User);

                // Manzilni saqlash yoki tanlash
                if (model.OrderType == "delivery")
                {
                    if (model.SelectedAddressId.HasValue)
                    {
                        addressId = model.SelectedAddressId;
                    }
                    else if (!string.IsNullOrEmpty(model.City) && user != null)
                    {
                        var addr = new Address
                        {
                            UserId = user.Id,
                            City   = model.City!,
                            Street = model.Street!,
                            Apartment = model.Apartment,
                            Phone  = model.DeliveryPhone!,
                        };
                        _db.Addresses.Add(addr);
                        await _db.SaveChangesAsync();
                        addressId = addr.Id;
                    }
                }
            }

            decimal subtotal    = cartItems.Sum(i => i.Price * i.Quantity);
            decimal deliveryFee = model.OrderType == "delivery" ? 15000 : 0;

            var order = new Order
            {
                OrderNumber   = "ORD" + DateTime.UtcNow.Ticks.ToString()[^8..],
                UserId        = user?.Id,
                GuestName     = user == null ? model.GuestName : null,
                GuestPhone    = user == null ? model.GuestPhone : null,
                GuestEmail    = user == null ? model.GuestEmail : null,
                OrderType     = model.OrderType == "delivery" ? OrderType.Delivery : OrderType.Pickup,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == "card" ? PaymentStatus.Paid : PaymentStatus.Pending,
                TotalAmount   = subtotal + deliveryFee,
                DeliveryFee   = deliveryFee,
                Notes         = model.Notes,
                AddressId     = addressId,
            };

            order.Items = cartItems.Select(i => new OrderItem
            {
                ProductId    = i.ProductId,
                ProductName  = i.Name,
                ProductUnit  = i.Unit,
                ProductImage = i.Image,
                UnitPrice    = i.Price,
                Quantity     = i.Quantity,
            }).ToList();

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Savatni tozalash
            SaveCart(new List<CartItem>());

            TempData["OrderId"] = order.Id;
            TempData["OrderNumber"] = order.OrderNumber;
            return RedirectToAction("Success");
        }

        // GET /Cart/Success
        public IActionResult Success()
        {
            ViewBag.OrderId = TempData["OrderId"];
            ViewBag.OrderNumber = TempData["OrderNumber"];
            return View();
        }
    }
}
