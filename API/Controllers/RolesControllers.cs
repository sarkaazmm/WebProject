using API.Dtos;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
[Authorize(Roles = "Admin")]//(Roles = "Admin, User...")
[ApiController]
[Route("api/[controller]")]
public class RolesControllers : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    public RolesControllers(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost("create")]
    public async Task<ActionResult<string>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        if (string.IsNullOrEmpty(createRoleDto.RoleName))
        {
            return BadRequest("Role name is required");
        }
        var roleExist = await _roleManager.FindByNameAsync(createRoleDto.RoleName);
        if (roleExist is not null)
        {
            return BadRequest("Role already exists");
        }
        var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));
        if (roleResult.Succeeded)
        {
            return Ok(new
            {
                IsSuccess = true,
                Message = "Role created successfully"
            });
        }
        return BadRequest("Role creation failed");
    }

    [HttpGet("all-roles")]
    public async Task<ActionResult<IEnumerable<RoleResponceDto>>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.Select(r => new RoleResponceDto
        {
            Id = r.Id,
            Name = r.Name,
            UserTotal = 0
        }).ToListAsync();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            role.UserTotal = usersInRole.Count;
        }
        return Ok(roles);
    }
    [HttpDelete("delete")]
    public async Task<ActionResult<string>> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return BadRequest("Role not found");
        }
        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            return Ok(new
            {
                IsSuccess = true,
                Message = "Role deleted successfully"
            });
        }
        return BadRequest("Role deletion failed");
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleDto.UserId!);
        if (user is null)
        {
            return BadRequest("User not found");
        }
        var role = await _roleManager.FindByIdAsync(assignRoleDto.RoleId!);
        if (role is null)
        {
            return BadRequest("Role not found");
        }
        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (result.Succeeded)
        {
            return Ok(new
            {
                IsSuccess = true,
                Message = "Role assigned successfully"
            });
        }
        return BadRequest("Role assignment failed");
    }
}



