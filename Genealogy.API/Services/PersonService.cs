using Microsoft.EntityFrameworkCore;
using Genealogy.API.Data;
using Genealogy.API.Dtos;
using Genealogy.API.Entities;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Services;

public class PersonService : IPersonService
{
    private readonly ApplicationDbContext _context;

    public PersonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PersonDto> CreatePersonAsync(PersonCreateDto personCreateDto, int userId)
    {
        // Проверяем существование пользователя
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var person = new Person
        {
            UserId = userId,
            FirstName = personCreateDto.FirstName,
            LastName = personCreateDto.LastName,
            MiddleName = personCreateDto.MiddleName,
            Gender = personCreateDto.Gender,
            BirthDate = personCreateDto.BirthDate,
            DeathDate = personCreateDto.DeathDate,
            Biography = personCreateDto.Biography,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        return MapToDto(person);
    }

    public async Task<PersonDto> GetPersonByIdAsync(int id)
    {
        var person = await _context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
            throw new KeyNotFoundException($"Person with ID {id} not found");

        return MapToDto(person);
    }

    public async Task<IEnumerable<PersonDto>> GetPersonsByUserIdAsync(int userId)
    {
        var persons = await _context.Persons
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .ToListAsync();

        return persons.Select(MapToDto);
    }

    public async Task<PersonDto> UpdatePersonAsync(int id, PersonUpdateDto personUpdateDto)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
            throw new KeyNotFoundException($"Person with ID {id} not found");

        // Обновляем поля
        if (!string.IsNullOrEmpty(personUpdateDto.FirstName))
            person.FirstName = personUpdateDto.FirstName;

        if (!string.IsNullOrEmpty(personUpdateDto.LastName))
            person.LastName = personUpdateDto.LastName;

        if (personUpdateDto.MiddleName != null)
            person.MiddleName = personUpdateDto.MiddleName;

        if (personUpdateDto.Gender.HasValue)
            person.Gender = personUpdateDto.Gender.Value;

        if (personUpdateDto.BirthDate.HasValue)
            person.BirthDate = personUpdateDto.BirthDate.Value;

        if (personUpdateDto.DeathDate.HasValue)
        {
            // Валидация: дата смерти должна быть после даты рождения
            if (person.BirthDate.HasValue && personUpdateDto.DeathDate.Value <= person.BirthDate.Value)
                throw new ArgumentException("Death date must be after birth date");
            person.DeathDate = personUpdateDto.DeathDate.Value;
        }

        if (personUpdateDto.Biography != null)
            person.Biography = personUpdateDto.Biography;

        person.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(person);
    }

    public async Task<bool> DeletePersonAsync(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
            throw new KeyNotFoundException($"Person with ID {id} not found");

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PersonDto>> GetAllPersonsAsync()
    {
        var persons = await _context.Persons
            .Include(p => p.User)
            .ToListAsync();

        return persons.Select(MapToDto);
    }

	public async Task<IEnumerable<PersonDto>> GetChildrenAsync(int personId)
	{
		var childrenIds = await _context.Relationships
			.Where(r => r.RelationshipType == RelationshipType.ParentChild &&
					r.RelatedPersonId == personId)
			.Select(r => r.PersonId)
			.ToListAsync();
		
		return await GetPersonsByIdsAsync(childrenIds);
	}

	public async Task<IEnumerable<PersonDto>> GetParentsAsync(int personId)
	{
		var parentIds = await _context.Relationships
			.Where(r => r.RelationshipType == RelationshipType.ParentChild &&
					r.PersonId == personId)
			.Select(r => r.RelatedPersonId)
			.ToListAsync();
		
		return await GetPersonsByIdsAsync(parentIds);
	}

	public async Task<IEnumerable<PersonDto>> GetSiblingsAsync(int personId)
	{
		// 1. Находим родителей
		var parentIds = await _context.Relationships
			.Where(r => r.RelationshipType == RelationshipType.ParentChild &&
					r.PersonId == personId)
			.Select(r => r.RelatedPersonId)
			.ToListAsync();
		
		// 2. Находим других детей этих родителей
		var siblingIds = await _context.Relationships
			.Where(r => r.RelationshipType == RelationshipType.ParentChild &&
					parentIds.Contains(r.RelatedPersonId) &&
					r.PersonId != personId)
			.Select(r => r.PersonId)
			.Distinct()
			.ToListAsync();
		
		return await GetPersonsByIdsAsync(siblingIds);
	}

	private async Task<IEnumerable<PersonDto>> GetPersonsByIdsAsync(List<int> ids)
	{
		if (!ids.Any())
			return new List<PersonDto>();
		
		var persons = await _context.Persons
			.Where(p => ids.Contains(p.Id))
			.ToListAsync();
		
		return persons.Select(MapToDto);
	}

    private static PersonDto MapToDto(Person person) => new()
    {
        Id = person.Id,
        UserId = person.UserId,
        FirstName = person.FirstName,
        LastName = person.LastName,
        MiddleName = person.MiddleName,
        Gender = person.Gender,
        BirthDate = person.BirthDate,
        DeathDate = person.DeathDate,
        Biography = person.Biography,
        CreatedAt = person.CreatedAt,
        UpdatedAt = person.UpdatedAt,
        Age = person.Age,
        IsAlive = person.IsAlive
    };
}