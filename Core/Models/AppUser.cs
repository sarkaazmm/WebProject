using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Core.Models
{
    [Table("AspNetUsers")]
    public class AppUser:IdentityUser
    {
        public required override string? UserName { get; set; }
        public required override string? Email { get; set; }
        public override string? PasswordHash { get; set; }   

    }
}