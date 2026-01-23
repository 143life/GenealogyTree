// Исправленный IPersonService.cs
using Genealogy.API.Dtos;
using Genealogy.API.Entities; // Добавьте эту строку

namespace Genealogy.API.Interfaces;

public interface IPersonService
{
    Task<PersonDto> CreatePersonAsync(PersonCreateDto personCreateDto, int userId);
    Task<PersonDto> GetPersonByIdAsync(int id);
    Task<IEnumerable<PersonDto>> GetPersonsByUserIdAsync(int userId);
    Task<PersonDto> UpdatePersonAsync(int id, PersonUpdateDto personUpdateDto);
    Task<bool> DeletePersonAsync(int id);
    Task<IEnumerable<PersonDto>> GetAllPersonsAsync();
    
    // ИЗМЕНИТЕ ТИП ВОЗВРАЩАЕМЫХ ЗНАЧЕНИЙ:
    Task<IEnumerable<PersonDto>> GetChildrenAsync(int personId);    // было Person
    Task<IEnumerable<PersonDto>> GetParentsAsync(int personId);     // было Person  
    Task<IEnumerable<PersonDto>> GetSiblingsAsync(int personId);    // было Person
}