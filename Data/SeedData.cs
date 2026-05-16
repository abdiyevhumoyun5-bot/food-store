using Microsoft.AspNetCore.Identity;
using Jahongir_diplomIshi.Models;

namespace Jahongir_diplomIshi.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Bazani tayyor qilish
            await context.Database.EnsureCreatedAsync();

            // Rollarni yaratish
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin foydalanuvchini yaratish
            const string adminEmail = "admin@foodstore.uz";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Jahongir Admin",
                    Phone = "+998915493307",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@1234");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Kategoriyalar
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Mevalar",           Slug = "mevalar" },
                    new() { Name = "Sabzavotlar",        Slug = "sabzavotlar" },
                    new() { Name = "Ichimliklar",        Slug = "ichimliklar" },
                    new() { Name = "Sut mahsulotlari",   Slug = "sut-mahsulotlari" },
                    new() { Name = "Non mahsulotlari",   Slug = "non-mahsulotlari" },
                    new() { Name = "Shirinliklar",       Slug = "shirinliklar" },
                    new() { Name = "Go'sht",             Slug = "gosht" },
                    new() { Name = "Don mahsulotlari",   Slug = "don-mahsulotlari" },
                    new() { Name = "Ko'katlar",          Slug = "kokatlar" },
                    new() { Name = "Poliz ekinlari",     Slug = "poliz-ekinlari" },
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Demo mahsulotlar
            if (!context.Products.Any())
            {
                var mevaId   = context.Categories.First(c => c.Slug == "mevalar").Id;
                var sabzId   = context.Categories.First(c => c.Slug == "sabzavotlar").Id;
                var ichimId  = context.Categories.First(c => c.Slug == "ichimliklar").Id;
                var sutId    = context.Categories.First(c => c.Slug == "sut-mahsulotlari").Id;
                var polizId  = context.Categories.First(c => c.Slug == "poliz-ekinlari").Id;

                var products = new List<Product>
                {
                    new() { Name = "Qovun",       CategoryId = polizId, Price = 8000,  Unit = "kg",   Stock = 50,  Seasons = "Summer",        IsFeatured = true,  CaloriesPer100g = 34,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/PNG_transparency_demonstration_1.png/280px-PNG_transparency_demonstration_1.png", Description = "Shirinligi bilan mashhur O'zbekiston qovuni." },
                    new() { Name = "Tarvuz",      CategoryId = polizId, Price = 5000,  Unit = "kg",   Stock = 80,  Seasons = "Summer",        IsFeatured = true,  CaloriesPer100g = 30,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/4c/Watermelon_seedless_Julian_Clifton.jpg", Description = "Yoz faslidagi eng sevimli meva." },
                    new() { Name = "Olma",        CategoryId = mevaId,  Price = 12000, Unit = "kg",   Stock = 100, Seasons = "Autumn,Winter", IsFeatured = true,  CaloriesPer100g = 52,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/15/Red_Apple.jpg", Description = "Vitaminlarga boy qizil olma." },
                    new() { Name = "Banan",       CategoryId = mevaId,  Price = 18000, Unit = "kg",   Stock = 60,  Seasons = "Spring,Summer,Autumn,Winter", IsFeatured = false, CaloriesPer100g = 89, ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/8/8a/Banana-Robusta.jpg", Description = "Energiya beruvchi tropik meva." },
                    new() { Name = "Pomidor",     CategoryId = sabzId,  Price = 9000,  Unit = "kg",   Stock = 120, Seasons = "Summer,Autumn", IsFeatured = false, CaloriesPer100g = 18,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/8/89/Tomato_je.jpg", Description = "Tabiiy qizil pomidor." },
                    new() { Name = "Bodring",     CategoryId = sabzId,  Price = 7000,  Unit = "kg",   Stock = 90,  Seasons = "Summer",        IsFeatured = false, CaloriesPer100g = 15,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/43/Cucumbers_sliced.jpg", Description = "Yangi bog'dan terilgan bodring." },
                    new() { Name = "Limonad",     CategoryId = ichimId, Price = 6000,  Unit = "litr", Stock = 200, Seasons = "Summer",        IsFeatured = true,  CaloriesPer100g = 40,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/b/b6/Limoncello_001.jpg", Description = "Salqin limonad." },
                    new() { Name = "Sut",         CategoryId = sutId,   Price = 8000,  Unit = "litr", Stock = 150, Seasons = "Spring,Summer,Autumn,Winter", IsFeatured = false, CaloriesPer100g = 61,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0e/Milk_glass.jpg", Description = "Yangi sigir suti." },
                    new() { Name = "Qovoq",       CategoryId = polizId, Price = 4000,  Unit = "kg",   Stock = 70,  Seasons = "Autumn",        IsFeatured = false, CaloriesPer100g = 26,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/1b/Orange_pumpkin.jpg", Description = "Kuz uchun qovoq." },
                    new() { Name = "Uzum",        CategoryId = mevaId,  Price = 22000, Unit = "kg",   Stock = 40,  Seasons = "Autumn",        IsFeatured = true,  CaloriesPer100g = 67,  ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/46/A_Glass_of_Grapes.jpg", Description = "Shirali O'zbekiston uzumi." },
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
