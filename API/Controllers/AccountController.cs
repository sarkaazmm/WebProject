using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers;

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
    //[HttpPost("login")]
    // public async Task<ActionResult<AuthResponceDto>> Login(LoginDto loginDto){
    //     var user = await _userManager.FindByEmailAsync(loginDto.Email);
    //     if(user is null){
    //         return BadRequest("User does not exist");
    //     }
        
    // }
}
