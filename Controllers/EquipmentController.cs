using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService equipmentService;

    public EquipmentController(IEquipmentService service)
    {
        equipmentService = service;
    }

    [HttpPost("assign")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Assign([FromBody] ZimmetOlusturDto dto)
    {
        (bool success, string message) result = await equipmentService.AssignAsync(dto);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpPut("return/{id}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Return(int id)
    {
        (bool success, string message) result = await equipmentService.ReturnAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpGet("getAllEquipment")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> GetAll()
    {
        List<ZimmetDto> allEquipment = await equipmentService.GetAllAsync();
        return Ok(allEquipment);
    }

    [HttpGet("getByPersonnel/{personnelId}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> GetByPersonnel(int personnelId)
    {
        List<ZimmetDto> found = await equipmentService.GetByPersonnelIdAsync(personnelId);
        return Ok(found);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "IkYonetici,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        (bool success, string message) result = await equipmentService.DeleteAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return NoContent();
    }

    [HttpGet("getMyEquipment")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetMyEquipment()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        List<ZimmetDto> myEquipment = await equipmentService.GetByPersonnelIdAsync(personnelId);
        return Ok(myEquipment);
    }
}
