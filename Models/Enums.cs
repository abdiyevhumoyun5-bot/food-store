namespace Jahongir_diplomIshi.Models
{
    public enum OrderStatus
    {
        Pending,        // Kutilmoqda
        Processing,     // Tayyorlanmoqda
        Ready,          // Tayyor
        Delivered,      // Yetkazildi
        Cancelled       // Bekor qilindi
    }

    public enum PaymentStatus
    {
        Pending,    // To'lov kutilmoqda
        Paid,       // To'landi
        Refunded    // Qaytarildi
    }

    public enum OrderType
    {
        Delivery,   // Yetkazib berish
        Pickup      // Olib ketish
    }

    public enum Season
    {
        Spring,     // Bahor
        Summer,     // Yoz
        Autumn,     // Kuz
        Winter      // Qish
    }
}
