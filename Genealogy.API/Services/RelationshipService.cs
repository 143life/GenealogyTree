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
		// 1. Валидация базовых правил
		await ValidateRelationshipAsync(relationshipCreateDto);
		
		// 2. Создание связи
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

    private async Task ValidateRelationshipAsync(RelationshipCreateDto dto)
	{
		// 1. Нельзя создать связь с самим собой
		if (dto.PersonId == dto.RelatedPersonId)
			throw new ArgumentException("Cannot create relationship to self");
		
		// 2. Проверяем существование персон
		var personExists = await _context.Persons.AnyAsync(p => p.Id == dto.PersonId);
		var relatedExists = await _context.Persons.AnyAsync(p => p.Id == dto.RelatedPersonId);
		
		if (!personExists || !relatedExists)
			throw new KeyNotFoundException("One or both persons not found");
		
		// 3. Проверяем дублирование связи
		var exists = await _context.Relationships
			.AnyAsync(r => r.PersonId == dto.PersonId && 
						r.RelatedPersonId == dto.RelatedPersonId &&
						r.RelationshipType == dto.RelationshipType);
		
		if (exists)
			throw new ArgumentException("Relationship already exists");
		
		// 4. Валидация по типу связи
		switch (dto.RelationshipType)
		{
			case RelationshipType.ParentChild:
				await ValidateParentChildAsync(dto);
				break;
			case RelationshipType.Spouse:
				await ValidateSpouseAsync(dto);
				break;
			case RelationshipType.Sibling:
				await ValidateSiblingAsync(dto);
				break;
		}
	}

	private async Task ValidateParentChildAsync(RelationshipCreateDto dto)
	{
		// Человек не может быть своим предком
		if (await IsAncestorAsync(dto.PersonId, dto.RelatedPersonId))
			throw new ArgumentException("Person cannot be ancestor of themselves");
	}

	private async Task ValidateSpouseAsync(RelationshipCreateDto dto)
	{
		// Проверяем, что у человека нет другого супруга
		var hasSpouse = await _context.Relationships
			.AnyAsync(r => (r.PersonId == dto.PersonId || r.RelatedPersonId == dto.PersonId) &&
						r.RelationshipType == RelationshipType.Spouse);
		
		if (hasSpouse)
			throw new ArgumentException("Person already has a spouse");
	}

	private async Task ValidateSiblingAsync(RelationshipCreateDto dto)
	{
		// Для братьев/сестер нужен общий родитель
		var personParents = await GetParentIdsAsync(dto.PersonId);
		var relatedParents = await GetParentIdsAsync(dto.RelatedPersonId);
		
		if (!personParents.Intersect(relatedParents).Any())
			throw new ArgumentException("Siblings must have at least one common parent");
	}

	private async Task<List<int>> GetParentIdsAsync(int personId)
	{
		return await _context.Relationships
			.Where(r => r.PersonId == personId && 
					r.RelationshipType == RelationshipType.ParentChild)
			.Select(r => r.RelatedPersonId)
			.ToListAsync();
	}

	private async Task<bool> IsAncestorAsync(int potentialAncestorId, int personId)
	{
		// Рекурсивно проверяем родителей
		var parents = await GetParentIdsAsync(personId);
		
		foreach (var parentId in parents)
		{
			if (parentId == potentialAncestorId)
				return true;
			
			if (await IsAncestorAsync(potentialAncestorId, parentId))
				return true;
		}
		
		return false;
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