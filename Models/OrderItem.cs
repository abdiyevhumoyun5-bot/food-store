using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jahongir_diplomIshi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; } = 1;

        // Buyurtma vaqtidagi narx (snapshot)
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Mahsulot ma'lumotlari snapshot (mahsulot o'chirilsa ham saqlansin)
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ProductUnit { get; set; } = "kg";

        public string? ProductImage { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int? ProductId { get; set; }      // null bo'lishi mumkin (o'chirilgan mahsulot)
        public Product? Product { get; set; }

        // Jami narx
        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
