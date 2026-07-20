using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IzinController : ControllerBase
{
    private readonly IIzinService izinServis;

    public IzinController(IIzinService service)
    {
        izinServis = service;
    }


    [HttpGet]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetAll()
    {
        List<IzinTalepDto> tumTalepler = await izinServis.GetAllAsync();
        return Ok(tumTalepler);
    }

    [HttpGet("bekleyen")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetBekleyenler()
    {
        List<IzinTalepDto> bekleyenler = await izinServis.GetBekleyenlerAsync();
        return Ok(bekleyenler);
    }

    [HttpGet("personel/{personelId}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetPersonelGecmisi(int personelId)
    {
        IzinOzetDto ozet = await izinServis.GetOzetAsync(personelId);
        if (ozet == null)
        {
            return NotFound();
        }
        return Ok(ozet);
    }

    [HttpPut("{id}/onayla")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Onayla(int id)
    {
        (bool basarili, string mesaj) sonuc = await izinServis.OnaylaAsync(id);
        if (sonuc.basarili == false)
        {
            return BadRequest(sonuc.mesaj);
        }
        return Ok(sonuc.mesaj);
    }

    [HttpPut("{id}/reddet")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Reddet(int id)
    {
        (bool basarili, string mesaj) sonuc = await izinServis.ReddedAsync(id);
        if (sonuc.basarili == false)
        {
            return BadRequest(sonuc.mesaj);
        }
        return Ok(sonuc.mesaj);
    }


    [HttpPost]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> TalepOlustur([FromBody] IzinTalepOlusturDto dto)
    {
        string personelIdMetni = User.FindFirstValue("PersonelId");
        if (personelIdMetni == null)
        {
            return Unauthorized();
        }

        int personelId = int.Parse(personelIdMetni);
        (bool basarili, string mesaj) sonuc = await izinServis.TalepOlusturAsync(personelId, dto);
        if (sonuc.basarili == false)
        {
            return BadRequest(sonuc.mesaj);
        }

        return Ok(sonuc.mesaj);
    }

    [HttpGet("benim")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetBenim()
    {
        string personelIdMetni = User.FindFirstValue("PersonelId");
        if (personelIdMetni == null)
        {
            return Unauthorized();
        }

        int personelId = int.Parse(personelIdMetni);
        List<IzinTalepDto> benimTaleplerim = await izinServis.GetByPersonelIdAsync(personelId);
        return Ok(benimTaleplerim);
    }

    [HttpGet("benim/ozet")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetBenimOzet()
    {
        string personelIdMetni = User.FindFirstValue("PersonelId");
        if (personelIdMetni == null)
        {
            return Unauthorized();
        }

        int personelId = int.Parse(personelIdMetni);
        IzinOzetDto izinOzetim = await izinServis.GetOzetAsync(personelId);
        if (izinOzetim == null)
        {
            return NotFound();
        }

        return Ok(izinOzetim);
    }
}
