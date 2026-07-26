using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Leave")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService leaveService;

    public LeaveController(ILeaveService service)
    {
        leaveService = service;
    }

    [HttpGet("getLeaves")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetAll()
    {
        List<IzinTalepDto> allRequests = await leaveService.GetAllAsync();
        return Ok(allRequests);
    }

    [HttpGet("getPending")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetPending()
    {
        List<IzinTalepDto> pending = await leaveService.GetPendingAsync();
        return Ok(pending);
    }

    [HttpGet("getPersonnelHistory/{personnelId}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetPersonnelHistory(int personnelId)
    {
        IzinOzetDto summary = await leaveService.GetSummaryAsync(personnelId);
        if (summary == null)
        {
            return NotFound();
        }
        return Ok(summary);
    }

    [HttpPut("approveLeave/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Approve(int id)
    {
        (bool success, string message) result = await leaveService.ApproveAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpPut("rejectLeave/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Reject(int id)
    {
        (bool success, string message) result = await leaveService.RejectAsync(id);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

    [HttpPost("createLeave")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> CreateRequest([FromBody] IzinTalepOlusturDto dto)
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        (bool success, string message) result = await leaveService.CreateRequestAsync(personnelId, dto);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }

        return Ok(result.message);
    }

    [HttpGet("getMyLeaves")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetMyLeaves()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        List<IzinTalepDto> myRequests = await leaveService.GetByPersonnelIdAsync(personnelId);
        return Ok(myRequests);
    }

    [HttpGet("getMySummary")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetMySummary()
    {
        string personnelIdText = User.FindFirstValue("PersonelId");
        if (personnelIdText == null)
        {
            return Unauthorized();
        }

        int personnelId = int.Parse(personnelIdText);
        IzinOzetDto mySummary = await leaveService.GetSummaryAsync(personnelId);
        if (mySummary == null)
        {
            return NotFound();
        }

        return Ok(mySummary);
    }
}
