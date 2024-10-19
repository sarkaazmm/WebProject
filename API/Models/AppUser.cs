using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    [Table("AspNetUsers")]
    public class AppUser:IdentityUser
    {
        public override string? UserName { get; set; }
        public override string? Email { get; set; }
        public override string? PasswordHash { get; set; }   
    }
}