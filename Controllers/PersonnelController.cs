using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Personnel")]
[Authorize]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService personnelService;

    public PersonnelController(IPersonnelService service)
    {
        personnelService = service;
    }

    [HttpGet("getPersonnel")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        List<PersonelDto> allPersonnel = await personnelService.GetAllAsync(includeInactive);
        return Ok(allPersonnel);
    }

    [HttpPut("reactivatePersonnel/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Reactivate(int id)
    {
        (bool success, string message) result = await personnelService.ReactivateAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpGet("getById/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetById(int id)
    {
        PersonelDto found = await personnelService.GetByIdAsync(id);
        if (found == null)
        {
            return NotFound();
        }
        return Ok(found);
    }

    [HttpGet("getByDepartment/{department}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetByDepartment(Departman department)
    {
        List<PersonelDto> sameDepartment = await personnelService.GetByDepartmentAsync(department);
        return Ok(sameDepartment);
    }

    [HttpGet("getBySalary")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetBySalary([FromQuery] bool descending = true)
    {
        List<PersonelDto> sortedList = await personnelService.GetOrderedBySalaryAsync(descending);
        return Ok(sortedList);
    }

    [HttpGet("search")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("Arama kelimesi boş olamaz.");
        }

        List<PersonelDto> found = await personnelService.SearchAsync(keyword);
        return Ok(found);
    }

    [HttpPost("addPersonnel")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Add([FromBody] PersonelCreateDto dto)
    {
        PersonelDto created = await personnelService.AddAsync(dto);
        return Created("", created);
    }

    [HttpPut("updatePersonnel/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonelUpdateDto dto)
    {
        PersonelDto? updated = await personnelService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFound();
        }
        return Ok(updated);
    }

    [HttpDelete("deletePersonnel/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Delete(int id)
    {
        bool success = await personnelService.DeleteAsync(id);
        if (success == false)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("getProfile")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetProfile()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        PersonelDto profile = await personnelService.GetByIdAsync(personnelId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("updateProfile")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> UpdateOwnProfile([FromBody] CalisanProfilUpdateDto dto)
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        bool success = await personnelService.UpdateOwnProfileAsync(personnelId, dto);
        if (success == false)
        {
            return NotFound();
        }

        return NoContent();
    }
}
