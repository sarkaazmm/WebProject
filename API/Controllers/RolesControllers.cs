using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
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
        public async Task<ActionResult<string>> CreateRole([FromBody] CreateRoleDto createRoleDto){
            if(string.IsNullOrEmpty(createRoleDto.RoleName)){
                return BadRequest("Role name is required");
            }
            var roleExist = await _roleManager.FindByNameAsync(createRoleDto.RoleName);
            if(roleExist is not null){
                return BadRequest("Role already exists");
            }
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));
            if(roleResult.Succeeded){
                return Ok(new {
                    IsSuccess = true, 
                    Message = "Role created successfully"
                });
            }
            return BadRequest("Role creation failed");
        }
    }

    
}