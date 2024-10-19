namespace API.Dtos
{
    public class UserDetailDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string[]? Roles { get; set; }
    }
}