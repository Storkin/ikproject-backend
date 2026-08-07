using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Salary")]
[Authorize]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService salaryService;

    public SalaryController(ISalaryService service)
    {
        salaryService = service;
    }

    [HttpPost("add")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Add([FromBody] MaasKaydiOlusturDto dto)
    {
        (bool success, string message) result = await salaryService.AddAsync(dto);
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
        (bool success, string message) result = await salaryService.DeleteAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return NoContent();
    }

    [HttpGet("getByPersonnel/{personnelId}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> GetByPersonnel(int personnelId)
    {
        List<MaasKaydiDto> found = await salaryService.GetByPersonnelIdAsync(personnelId);
        return Ok(found);
    }

    [HttpGet("getMySalaryHistory")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetMySalaryHistory()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        List<MaasKaydiDto> myHistory = await salaryService.GetByPersonnelIdAsync(personnelId);
        return Ok(myHistory);
    }
}
