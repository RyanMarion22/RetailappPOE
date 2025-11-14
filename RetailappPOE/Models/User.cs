using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RetailappPOE.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MinLength(3)]
        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;   // hashed

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        public string Role { get; set; } = "Customer";

        // Used only for registration form – not stored in DB
        [NotMapped]
        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
    }
}