using System.ComponentModel.DataAnnotations;
using Genealogy.API.Entities;

namespace Genealogy.API.Dtos;

public class RelationshipDto
{
    public int Id { get; set; }
    
    [Required]
    public int PersonId { get; set; }
    
    [Required]
    public int RelatedPersonId { get; set; }
    
    [Required]
    public RelationshipType RelationshipType { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

public class RelationshipCreateDto
{
    [Required]
    public int PersonId { get; set; }
    
    [Required]
    public int RelatedPersonId { get; set; }
    
    [Required]
    public RelationshipType RelationshipType { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

public class RelationshipUpdateDto
{
    [MaxLength(500)]
    public string? Note { get; set; }
}