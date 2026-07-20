using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkProjesi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuyuruController : ControllerBase
{
    private readonly IDuyuruService duyuruServis;

    public DuyuruController(IDuyuruService service)
    {
        duyuruServis = service;
    }

    [HttpGet]
    [Authorize(Roles = "IkYonetici,Calisan")]
    public async Task<IActionResult> GetAll()
    {
        List<DuyuruDto> tumDuyurular = await duyuruServis.GetAllAsync();
        return Ok(tumDuyurular);
    }

    [HttpPost]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Add([FromBody] DuyuruOlusturDto dto)
    {
        await duyuruServis.AddAsync(dto);
        return Created();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Delete(int id)
    {
        bool islemBasarili = await duyuruServis.DeleteAsync(id);
        if (islemBasarili == false)
        {
            return NotFound();
        }
        return NoContent();
    }
}
