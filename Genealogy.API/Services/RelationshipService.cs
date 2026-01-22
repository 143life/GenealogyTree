using Microsoft.EntityFrameworkCore;
using Genealogy.API.Data;
using Genealogy.API.Dtos;
using Genealogy.API.Entities;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Services;

public class RelationshipService : IRelationshipService
{
    private readonly ApplicationDbContext _context;

    public RelationshipService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RelationshipDto> CreateRelationshipAsync(RelationshipCreateDto relationshipCreateDto)
    {
        // Проверяем, что оба человека существуют
        var personExists = await _context.Persons.AnyAsync(p => p.Id == relationshipCreateDto.PersonId);
        var relatedPersonExists = await _context.Persons.AnyAsync(p => p.Id == relationshipCreateDto.RelatedPersonId);

        if (!personExists || !relatedPersonExists)
            throw new KeyNotFoundException("One or both persons not found");

        // Проверяем, что связь не является самоссылкой
        if (relationshipCreateDto.PersonId == relationshipCreateDto.RelatedPersonId)
            throw new ArgumentException("Cannot create relationship to the same person");

        var relationship = new Relationship
        {
            PersonId = relationshipCreateDto.PersonId,
            RelatedPersonId = relationshipCreateDto.RelatedPersonId,
            RelationshipType = relationshipCreateDto.RelationshipType,
            Note = relationshipCreateDto.Note
        };

        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        return MapToDto(relationship);
    }

    public async Task<RelationshipDto> GetRelationshipByIdAsync(int id)
    {
        var relationship = await _context.Relationships
            .Include(r => r.Person)
            .Include(r => r.RelatedPerson)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (relationship == null)
            throw new KeyNotFoundException($"Relationship with ID {id} not found");

        return MapToDto(relationship);
    }

    public async Task<IEnumerable<RelationshipDto>> GetRelationshipsByPersonIdAsync(int personId)
    {
        var relationships = await _context.Relationships
            .Where(r => r.PersonId == personId || r.RelatedPersonId == personId)
            .Include(r => r.Person)
            .Include(r => r.RelatedPerson)
            .ToListAsync();

        return relationships.Select(MapToDto);
    }

    public async Task<RelationshipDto> UpdateRelationshipAsync(int id, RelationshipUpdateDto relationshipUpdateDto)
    {
        var relationship = await _context.Relationships.FindAsync(id);
        if (relationship == null)
            throw new KeyNotFoundException($"Relationship with ID {id} not found");

        if (relationshipUpdateDto.Note != null)
            relationship.Note = relationshipUpdateDto.Note;

        await _context.SaveChangesAsync();

        return MapToDto(relationship);
    }

    public async Task<bool> DeleteRelationshipAsync(int id)
    {
        var relationship = await _context.Relationships.FindAsync(id);
        if (relationship == null)
            throw new KeyNotFoundException($"Relationship with ID {id} not found");

        _context.Relationships.Remove(relationship);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ValidateRelationshipAsync(RelationshipCreateDto relationshipCreateDto)
    {
        // Проверяем, что оба человека существуют
        var personExists = await _context.Persons.AnyAsync(p => p.Id == relationshipCreateDto.PersonId);
        var relatedPersonExists = await _context.Persons.AnyAsync(p => p.Id == relationshipCreateDto.RelatedPersonId);

        if (!personExists || !relatedPersonExists)
            return false;

        // Проверяем, что связь не является самоссылкой
        if (relationshipCreateDto.PersonId == relationshipCreateDto.RelatedPersonId)
            return false;

        // Проверяем, что такая связь еще не существует
        var existingRelationship = await _context.Relationships
            .FirstOrDefaultAsync(r => r.PersonId == relationshipCreateDto.PersonId && 
                                      r.RelatedPersonId == relationshipCreateDto.RelatedPersonId &&
                                      r.RelationshipType == relationshipCreateDto.RelationshipType);

        return existingRelationship == null;
    }

    private static RelationshipDto MapToDto(Relationship relationship) => new()
    {
        Id = relationship.Id,
        PersonId = relationship.PersonId,
        RelatedPersonId = relationship.RelatedPersonId,
        RelationshipType = relationship.RelationshipType,
        Note = relationship.Note
    };
}