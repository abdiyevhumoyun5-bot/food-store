using System.ComponentModel.DataAnnotations;
using Jahongir_diplomIshi.Models;

namespace Jahongir_diplomIshi.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Mahsulot nomi kiritilmadi")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Narx kiritilmadi")]
        [Range(1, 100000000, ErrorMessage = "Narx 1 dan yuqori bo'lishi kerak")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "O'lchov birligi tanlang")]
        public string Unit { get; set; } = "kg";

        [Range(0, 100000, ErrorMessage = "Ombor miqdori 0 dan yuqori bo'lishi kerak")]
        public int Stock { get; set; }

        // Rasm — URL yoki fayl
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        // Mavsumlar
        public List<string> SelectedSeasons { get; set; } = new();

        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "Kategoriya tanlang")]
        public int CategoryId { get; set; }

        public int? CaloriesPer100g { get; set; }

        // View uchun
        public List<Category> Categories { get; set; } = new();
    }

    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Search { get; set; }
        public string? CategorySlug { get; set; }
        public string? Season { get; set; }
    }
}
