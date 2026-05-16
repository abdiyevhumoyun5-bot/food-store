namespace Jahongir_diplomIshi.Services
{
    /// <summary>
    /// Rasm yuklash servisi — wwwroot/uploads/ papkasiga saqlaydi.
    /// </summary>
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Faylni wwwroot/uploads/ ga saqlaydi. Fayl nomi qaytariladi (yo'l emas).
        /// </summary>
        public async Task<string?> SaveImageAsync(IFormFile file, string subfolder = "products")
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("Fayl hajmi 5 MB dan oshmasligi kerak.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("Faqat JPG, PNG, GIF va WEBP formatlar qabul qilinadi.");

            // Papka yaratish
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadDir);

            // Noyob nom
            var fileName = $"{subfolder}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{subfolder}/{fileName}";
        }

        /// <summary>
        /// Faylni o'chirish
        /// </summary>
        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            var fullPath = Path.Combine(_env.WebRootPath, "uploads", imagePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
