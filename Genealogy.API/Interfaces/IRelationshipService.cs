using Genealogy.API.Dtos;

namespace Genealogy.API.Interfaces;

public interface IRelationshipService
{
    Task<RelationshipDto> CreateRelationshipAsync(RelationshipCreateDto relationshipCreateDto);
    Task<RelationshipDto> GetRelationshipByIdAsync(int id);
    Task<IEnumerable<RelationshipDto>> GetRelationshipsByPersonIdAsync(int personId);
    Task<RelationshipDto> UpdateRelationshipAsync(int id, RelationshipUpdateDto relationshipUpdateDto);
    Task<bool> DeleteRelationshipAsync(int id);
    Task<bool> ValidateRelationshipAsync(RelationshipCreateDto relationshipCreateDto);
}