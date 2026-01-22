using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Genealogy.API.Data;
using Genealogy.API.Dtos;
using Genealogy.API.Entities;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UserDto> RegisterAsync(UserCreateDto userCreateDto)
    {
        // Проверка существования пользователя
        if (await _context.Users.AnyAsync(u => u.Username == userCreateDto.Username))
            throw new ArgumentException("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == userCreateDto.Email))
            throw new ArgumentException("Email already exists");

        // Создание пользователя
        var user = new User
        {
            Username = userCreateDto.Username,
            Email = userCreateDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password),
            Role = Enum.Parse<UserRole>(userCreateDto.Role),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<TokenResponseDto> LoginAsync(UserLoginDto userLoginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == userLoginDto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        var token = GenerateJwtToken(user);

		return new TokenResponseDto
		{
			Token = token,
			Expires = DateTime.UtcNow.AddMinutes(
				Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
			User = MapToDto(user)
		};
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UserUpdateDto userUpdateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        // Обновляем поля
        if (!string.IsNullOrEmpty(userUpdateDto.Username))
        {
            if (await _context.Users.AnyAsync(u => u.Username == userUpdateDto.Username && u.Id != id))
                throw new ArgumentException("Username already exists");
            user.Username = userUpdateDto.Username;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Email))
        {
            if (await _context.Users.AnyAsync(u => u.Email == userUpdateDto.Email && u.Id != id))
                throw new ArgumentException("Email already exists");
            user.Email = userUpdateDto.Email;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);

        if (userUpdateDto.IsActive.HasValue)
            user.IsActive = userUpdateDto.IsActive.Value;

        if (!string.IsNullOrEmpty(userUpdateDto.Role))
            user.Role = Enum.Parse<UserRole>(userUpdateDto.Role);

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal userClaims)
    {
        var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return await GetUserByIdAsync(int.Parse(userId));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}