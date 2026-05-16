using System.ComponentModel.DataAnnotations;

namespace Jahongir_diplomIshi.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Street { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Apartment { get; set; }

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Label { get; set; }   // "Uy", "Ish" va h.k.

        public bool IsDefault { get; set; } = false;

        // Foreign Key
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
