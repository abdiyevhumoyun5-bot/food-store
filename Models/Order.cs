using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jahongir_diplomIshi.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Buyurtma raqami (ko'rsatish uchun)
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public OrderType OrderType { get; set; } = OrderType.Delivery;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // To'lov usuli
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }  // "card", "cash"

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryFee { get; set; } = 0;

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Guest buyurtma uchun (login qilmagan foydalanuvchi)
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }

        // Foreign Keys
        public string? UserId { get; set; }
        public User? User { get; set; }

        public int? AddressId { get; set; }
        public Address? Address { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
