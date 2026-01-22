using System.ComponentModel.DataAnnotations;
using Genealogy.API.Entities;

namespace Genealogy.API.Dtos;

public class PersonDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    
    [MaxLength(2000)]
    public string? Biography { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int? Age { get; set; }
    public bool IsAlive { get; set; }
}

public class PersonCreateDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    
    [MaxLength(2000)]
    public string? Biography { get; set; }
}

public class PersonUpdateDto
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [MaxLength(100)]
    public string? MiddleName { get; set; }
    
    public Gender? Gender { get; set; }
    
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    
    [MaxLength(2000)]
    public string? Biography { get; set; }
}