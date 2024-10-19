using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace API.Dtos
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } =  string.Empty;
        
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public List<string>? Roles { get; set; } = [];
    }
}