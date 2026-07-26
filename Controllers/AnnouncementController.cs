using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Announcement")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService announcementService;

    public AnnouncementController(IAnnouncementService service)
    {
        announcementService = service;
    }

    [HttpGet("getAnnouncements")]
    [Authorize(Roles = "IkYonetici,Calisan")]
    public async Task<IActionResult> GetAll()
    {
        List<DuyuruDto> allAnnouncements = await announcementService.GetAllAsync();
        return Ok(allAnnouncements);
    }

    [HttpPost("createAnnouncement")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Add([FromBody] DuyuruOlusturDto dto)
    {
        DuyuruDto created = await announcementService.AddAsync(dto);
        return Created("", created);
    }

    [HttpPut("updateAnnouncement/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Update(int id, [FromBody] DuyuruUpdateDto dto)
    {
        DuyuruDto? updated = await announcementService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFound();
        }
        return Ok(updated);
    }

    [HttpDelete("deleteAnnouncement/{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Delete(int id)
    {
        bool success = await announcementService.DeleteAsync(id);
        if (success == false)
        {
            return NotFound();
        }
        return NoContent();
    }
}
