using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Education")]
[Authorize]
public class EducationController : ControllerBase
{
    private readonly IEducationService educationService;

    public EducationController(IEducationService service)
    {
        educationService = service;
    }

    [HttpPost("add")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Add([FromBody] EgitimOlusturDto dto)
    {
        (bool success, string message) result = await educationService.AddAsync(dto);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EgitimGuncelleDto dto)
    {
        (bool success, string message) result = await educationService.UpdateAsync(id, dto);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        (bool success, string message) result = await educationService.DeleteAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return NoContent();
    }

    [HttpGet("getAllEducation")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> GetAll()
    {
        List<EgitimDto> allRecords = await educationService.GetAllAsync();
        return Ok(allRecords);
    }

    [HttpGet("getByPersonnel/{personnelId}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> GetByPersonnel(int personnelId)
    {
        List<EgitimDto> found = await educationService.GetByPersonnelIdAsync(personnelId);
        return Ok(found);
    }

    [HttpGet("getMyEducation")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetMyEducation()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        List<EgitimDto> myEducation = await educationService.GetByPersonnelIdAsync(personnelId);
        return Ok(myEducation);
    }
}
