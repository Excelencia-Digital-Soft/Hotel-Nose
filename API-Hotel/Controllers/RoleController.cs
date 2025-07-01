using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotel.Models.Identity;

namespace hotel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class RoleController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost("initialize-roles")]
    public async Task<IActionResult> InitializeRoles()
    {
        try
        {
            var roles = new[]
            {
                new ApplicationRole { Name = "Administrator", Description = "System Administrator with full access" },
                new ApplicationRole { Name = "Manager", Description = "Hotel Manager with management privileges" },
                new ApplicationRole { Name = "Employee", Description = "Hotel Employee with limited access" },
                new ApplicationRole { Name = "User", Description = "Basic user with minimal access" }
            };

            var createdRoles = new List<string>();

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.Name!))
                {
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        createdRoles.Add(role.Name!);
                    }
                }
            }

            return Ok(new
            {
                message = "Roles initialization completed",
                createdRoles = createdRoles
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error initializing roles", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationRole>>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles);
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return NotFound(new { message = "Role not found" });
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
        {
            return Ok(new { message = $"Role '{roleName}' assigned to user successfully" });
        }

        return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (result.Succeeded)
        {
            return Ok(new { message = $"Role '{roleName}' removed from user successfully" });
        }

        return BadRequest(new { message = "Failed to remove role", errors = result.Errors });
    }
}