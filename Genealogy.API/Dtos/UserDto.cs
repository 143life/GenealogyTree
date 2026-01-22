using System.ComponentModel.DataAnnotations;

namespace Genealogy.API.Dtos;

public class UserDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    public string Role { get; set; } = "User";
}

public class UserUpdateDto
{
    [MaxLength(50)]
    public string? Username { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MinLength(6)]
    public string? Password { get; set; }
    
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class UserLoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserDto User { get; set; } = null!;
}