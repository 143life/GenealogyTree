using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Genealogy.API.Dtos;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RelationshipsController : ControllerBase
{
    private readonly IRelationshipService _relationshipService;

    public RelationshipsController(IRelationshipService relationshipService)
    {
        _relationshipService = relationshipService;
    }

    [HttpPost]
    public async Task<ActionResult<RelationshipDto>> CreateRelationship(RelationshipCreateDto relationshipCreateDto)
    {
        try
        {
            var relationship = await _relationshipService.CreateRelationshipAsync(relationshipCreateDto);
            return CreatedAtAction(nameof(GetRelationship), new { id = relationship.Id }, relationship);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RelationshipDto>> GetRelationship(int id)
    {
        try
        {
            var relationship = await _relationshipService.GetRelationshipByIdAsync(id);
            return Ok(relationship);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("person/{personId}")]
    public async Task<ActionResult<IEnumerable<RelationshipDto>>> GetRelationshipsByPerson(int personId)
    {
        try
        {
            var relationships = await _relationshipService.GetRelationshipsByPersonIdAsync(personId);
            return Ok(relationships);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RelationshipDto>> UpdateRelationship(int id, RelationshipUpdateDto relationshipUpdateDto)
    {
        try
        {
            var relationship = await _relationshipService.UpdateRelationshipAsync(id, relationshipUpdateDto);
            return Ok(relationship);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRelationship(int id)
    {
        try
        {
            await _relationshipService.DeleteRelationshipAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateRelationship(RelationshipCreateDto relationshipCreateDto)
    {
        try
        {
            var isValid = await _relationshipService.ValidateRelationshipAsync(relationshipCreateDto);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}