using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
// api/account
public class AccountController:ControllerBase
{

    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration){
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    //api/account/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(RegisterDto registerDto){
        var userExist = await _userManager.FindByEmailAsync(registerDto.Email);
        if(userExist is not null){
            return BadRequest("User already exists");
        }
        var user = new AppUser{
            Email = registerDto.Email,
            UserName = registerDto.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.PasswordHash)
        };

        var result = await _userManager.CreateAsync(user);
        if(!result.Succeeded){
            return BadRequest(result.Errors);
        }

        if (registerDto.Roles is null){
            await _userManager.AddToRoleAsync(user, "User");
        }
        else{
            foreach(var role in registerDto.Roles){
                await _userManager.AddToRoleAsync(user, role);
            }
        }
        return Ok(new AuthResponceDto{
            IsSuccess = true,
            Message = "Account created successfully"
            });
    }

    //api/account/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponceDto>> Login(LoginDto loginDto){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        var user = await _userManager.FindByEmailAsync(loginDto.UserIdentifier);
        if (user == null) user = await _userManager.FindByNameAsync(loginDto.UserIdentifier);
        
        if(user is null){
            return Unauthorized(new AuthResponceDto{
                IsSuccess = false,
                Message = "User with this email or username does not exist"
            });
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized("The password does not match the user");
        }

        var token = GenerateToken(user);
        return Ok(new AuthResponceDto{
            IsSuccess = true,
            Token = token,
            Message = "Login successful"
        });
    }

    private string GenerateToken(AppUser user){
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII
        .GetBytes(_configuration.GetSection("JWTSettings").GetSection("SecretKey").Value!);

        var userRoles = _userManager.GetRolesAsync(user).Result;
        List<Claim> claims = [
            new (JwtRegisteredClaimNames.Email, user.Email ??""),
            new (JwtRegisteredClaimNames.Name, user.UserName ??""),
            new (JwtRegisteredClaimNames.NameId, user.Id ??""),
            new (JwtRegisteredClaimNames.Aud, _configuration.GetSection("JWTSettings").GetSection("Audience").Value!),
            new (JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSettings").GetSection("Issuer").Value!)
        ];
        foreach(var role in userRoles){
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    //api/account/detail
    [HttpGet("detail")]
    [Authorize]
    public async Task<ActionResult<AppUser>> GetUserDetail(){
        var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(currentUser!);

        if(user is null){
            return NotFound(new AuthResponceDto{
                IsSuccess = false,
                Message = "User not found"
            });
        }

        return Ok(new UserDetailDto{
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = [..await _userManager.GetRolesAsync(user)]
        });
    }

    [HttpGet("all-users-details")]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers(){
        var users = await _userManager.Users.Select(u => new UserDetailDto
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName,
            Roles = new string[] {} 
        }).ToListAsync();

        foreach (var user in users)
        {
            var userEntity = await _userManager.FindByIdAsync(user.Id!);
            if (userEntity is null) continue;
            var roles = await _userManager.GetRolesAsync(userEntity);
            user.Roles = roles.ToArray();
        }
        return Ok(users);
    }
}
