using Jahongir_diplomIshi.Models;

namespace Jahongir_diplomIshi.Services
{
    /// <summary>
    /// Kaloriya tahlili uchun servis.
    /// </summary>
    public class CalorieService
    {
        // Mahsulot nomi bo'yicha 100g kaloriyas (taxminiy)
        private static readonly Dictionary<string, int> CalorieDb = new(StringComparer.OrdinalIgnoreCase)
        {
            { "olma", 52 }, { "nok", 57 }, { "banan", 89 }, { "uzum", 67 },
            { "apelsin", 47 }, { "limon", 29 }, { "gilos", 63 }, { "shaftoli", 39 },
            { "qovun", 34 }, { "tarvuz", 30 }, { "anor", 83 }, { "o'rik", 48 },
            { "pomidor", 18 }, { "bodring", 15 }, { "karam", 25 }, { "sabzi", 41 },
            { "kartoshka", 77 }, { "piyoz", 40 }, { "sarimsoq", 149 }, { "qovoq", 26 },
            { "sut", 61 }, { "qatiq", 59 }, { "kefir", 41 }, { "tvorog", 103 },
            { "pishloq", 350 }, { "sariyog", 717 }, { "tuxum", 155 },
            { "go'sht", 250 }, { "mol go'shti", 270 }, { "tovuq", 165 }, { "baliq", 200 },
            { "guruch", 130 }, { "bug'doy", 327 }, { "non", 265 }, { "makaron", 131 },
            { "shakar", 387 }, { "asal", 304 }, { "shirin", 400 },
            { "limonad", 40 }, { "sharbat", 45 }, { "suv", 0 },
        };

        public int GetCaloriesPer100g(string productName)
        {
            // To'g'ri topilsa qaytaradi
            if (CalorieDb.TryGetValue(productName.Trim(), out int cal))
                return cal;

            // Qisman mos
            var lower = productName.ToLower();
            foreach (var kv in CalorieDb)
                if (lower.Contains(kv.Key) || kv.Key.Contains(lower))
                    return kv.Value;

            return 50; // Standart (aniqlanmagan)
        }

        public CalorieAnalysisResult AnalyzeOrders(IEnumerable<Order> orders)
        {
            var result = new CalorieAnalysisResult();

            var orderedItems = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items)
                .ToList();

            if (!orderedItems.Any())
                return result;

            // Eng ko'p buyurtma qilingan mahsulotlar
            result.TopProducts = orderedItems
                .GroupBy(i => i.ProductName)
                .Select(g => new ProductCalorieStat
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    CaloriesPer100g = GetCaloriesPer100g(g.Key)
                })
                .OrderByDescending(p => p.Quantity)
                .Take(5)
                .ToList();

            // Jami taxminiy kaloriya (1 birlik = 200g deb hisoblaymiz)
            result.TotalCalories = orderedItems.Sum(i =>
                GetCaloriesPer100g(i.ProductName) * i.Quantity * 2);

            int deliveredOrdersCount = orders.Count(o => o.Status == OrderStatus.Delivered);
            result.AvgDailyCalories = deliveredOrdersCount > 0
                ? result.TotalCalories / deliveredOrdersCount
                : 0;

            // Tavsiya
            result.Recommendation = result.AvgDailyCalories switch
            {
                < 1200 => "Kunlik kaloriya iste'molingiz juda past. Ko'proq ovqatlanishga harakat qiling.",
                < 1800 => "Kaloriya iste'molingiz maqbul darajada. Yaxshi natijalarga erishasiz!",
                < 2500 => "Kaloriya me'yorida. Sog'lom va muvozanatli ovqatlanishda davom eting.",
                _      => "Kunlik kaloriya iste'moli yuqori. Kamroq kaloriyali mahsulotlarni tanlang."
            };

            return result;
        }
    }

    public class CalorieAnalysisResult
    {
        public int TotalCalories { get; set; }
        public int AvgDailyCalories { get; set; }
        public string Recommendation { get; set; } = "";
        public List<ProductCalorieStat> TopProducts { get; set; } = new();
    }

    public class ProductCalorieStat
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public int CaloriesPer100g { get; set; }
    }
}
