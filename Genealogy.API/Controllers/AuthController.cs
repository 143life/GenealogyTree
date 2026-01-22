using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Genealogy.API.Dtos;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(UserCreateDto userCreateDto)
    {
        try
        {
            var user = await _userService.RegisterAsync(userCreateDto);
            return CreatedAtAction(nameof(GetCurrentUser), user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("login")]
	public async Task<ActionResult<TokenResponseDto>> Login(UserLoginDto userLoginDto)
	{
		try
		{
			var tokenResponse = await _userService.LoginAsync(userLoginDto);
			return Ok(tokenResponse);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { message = "Internal server error", error = ex.Message });
		}
	}

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var user = await _userService.GetCurrentUserAsync(User);
            return Ok(user);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}