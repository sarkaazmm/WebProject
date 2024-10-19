using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class LoginDto
    {
        [Required]
        public string UserIdentifier { get; set; } =  string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public List<string>? Roles { get; set; } = [];
    }
}