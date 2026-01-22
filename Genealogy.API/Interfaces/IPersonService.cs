using Genealogy.API.Dtos;

namespace Genealogy.API.Interfaces;

public interface IPersonService
{
    Task<PersonDto> CreatePersonAsync(PersonCreateDto personCreateDto, int userId);
    Task<PersonDto> GetPersonByIdAsync(int id);
    Task<IEnumerable<PersonDto>> GetPersonsByUserIdAsync(int userId);
    Task<PersonDto> UpdatePersonAsync(int id, PersonUpdateDto personUpdateDto);
    Task<bool> DeletePersonAsync(int id);
    Task<IEnumerable<PersonDto>> GetAllPersonsAsync();
}