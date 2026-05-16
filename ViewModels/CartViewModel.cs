using System.ComponentModel.DataAnnotations;

namespace Jahongir_diplomIshi.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = "kg";
        public string? Image { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);
        public decimal DeliveryFee { get; set; } = 15000;
        public decimal Total => Subtotal + (OrderType == "delivery" ? DeliveryFee : 0);
        public string OrderType { get; set; } = "delivery"; // delivery | pickup
    }

    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; } = 15000;
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Buyurtma turini tanlang")]
        public string OrderType { get; set; } = "delivery";

        // Yetkazib berish manzili
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Apartment { get; set; }
        public string? DeliveryPhone { get; set; }

        // Mehmon foydalanuvchi uchun
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }

        // To'lov
        [Required(ErrorMessage = "To'lov usulini tanlang")]
        public string PaymentMethod { get; set; } = "cash";

        // Karta ma'lumotlari (card to'lov uchun)
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvc { get; set; }

        public string? Notes { get; set; }

        // Saqlangan manzillar (login foydalanuvchi uchun)
        public List<Jahongir_diplomIshi.Models.Address> SavedAddresses { get; set; } = new();
        public int? SelectedAddressId { get; set; }
    }
}
