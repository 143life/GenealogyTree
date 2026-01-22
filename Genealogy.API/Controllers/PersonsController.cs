using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Genealogy.API.Dtos;
using Genealogy.API.Interfaces;

namespace Genealogy.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpPost]
    public async Task<ActionResult<PersonDto>> CreatePerson(PersonCreateDto personCreateDto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var person = await _personService.CreatePersonAsync(personCreateDto, userId);
            return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
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
    public async Task<ActionResult<PersonDto>> GetPerson(int id)
    {
        try
        {
            var person = await _personService.GetPersonByIdAsync(id);
            return Ok(person);
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonDto>>> GetMyPersons()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var persons = await _personService.GetPersonsByUserIdAsync(userId);
            return Ok(persons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PersonDto>>> GetAllPersons()
    {
        try
        {
            var persons = await _personService.GetAllPersonsAsync();
            return Ok(persons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PersonDto>> UpdatePerson(int id, PersonUpdateDto personUpdateDto)
    {
        try
        {
            var person = await _personService.UpdatePersonAsync(id, personUpdateDto);
            return Ok(person);
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePerson(int id)
    {
        try
        {
            await _personService.DeletePersonAsync(id);
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
}