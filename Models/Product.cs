using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jahongir_diplomIshi.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "kg";  // kg, dona, litr, paket, gramm, ml

        public int Stock { get; set; } = 0;

        // Rasm — URL yoki yuklangan fayl
        public string? ImageUrl { get; set; }       // URL orqali kiritilgan rasm
        public string? ImagePath { get; set; }      // IFormFile bilan yuklangan rasm (wwwroot/uploads/)

        // Mavsumlar — vergul bilan: "Summer,Autumn"
        public string? Seasons { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Kaloriya (100g uchun)
        public int? CaloriesPer100g { get; set; }

        // Foreign Key
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Computed: rasm URL-ni qaytaradi (yuklangan fayl ustunlik qiladi)
        [NotMapped]
        public string DisplayImage =>
            !string.IsNullOrEmpty(ImagePath) ? "/uploads/" + ImagePath :
            !string.IsNullOrEmpty(ImageUrl) ? ImageUrl :
            "/img/no-image.png";

        // Mavsumlar ro'yxat sifatida
        [NotMapped]
        public List<Season> SeasonList
        {
            get
            {
                if (string.IsNullOrEmpty(Seasons)) return new List<Season>();
                return Seasons.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Enum.TryParse<Season>(s.Trim(), out var result) ? result : (Season?)null)
                    .Where(s => s.HasValue)
                    .Select(s => s!.Value)
                    .ToList();
            }
        }
    }
}
