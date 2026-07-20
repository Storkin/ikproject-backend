using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonelController : ControllerBase
{
    private readonly IPersonelService personelServis;

    public PersonelController(IPersonelService service)
    {
        personelServis = service;
    }


    [HttpGet]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetAll()
    {
        List<PersonelDto> tumPersoneller = await personelServis.GetAllAsync();
        return Ok(tumPersoneller);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetById(int id)
    {
        PersonelDto bulunanPersonel = await personelServis.GetByIdAsync(id);
        if (bulunanPersonel == null)
        {
            return NotFound();
        }
        return Ok(bulunanPersonel);
    }

    [HttpGet("departman/{departman}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetByDepartman(string departman)
    {
        List<PersonelDto> ayniDepartmandakiler = await personelServis.GetByDepartmanAsync(departman);
        return Ok(ayniDepartmandakiler);
    }

    [HttpGet("maas")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> GetByMaas([FromQuery] bool azalan = true)
    {
        List<PersonelDto> siraliListe = await personelServis.GetOrderedByMaasAsync(azalan);
        return Ok(siraliListe);
    }

    [HttpGet("ara")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Ara([FromQuery] string kelime)
    {
        if (string.IsNullOrWhiteSpace(kelime))
        {
            return BadRequest("Arama kelimesi boş olamaz.");
        }

        List<PersonelDto> bulunanlar = await personelServis.AraAsync(kelime);
        return Ok(bulunanlar);
    }

    [HttpPost]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Add([FromBody] PersonelCreateDto dto)
    {
        await personelServis.AddAsync(dto);
        return Created();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonelUpdateDto dto)
    {
        bool islemBasarili = await personelServis.UpdateAsync(id, dto);
        if (islemBasarili == false)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> Delete(int id)
    {
        bool islemBasarili = await personelServis.DeleteAsync(id);
        if (islemBasarili == false)
        {
            return NotFound();
        }
        return NoContent();
    }


    [HttpGet("profil")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> GetProfil()
    {
        string personelIdMetni = User.FindFirstValue("PersonelId");
        if (personelIdMetni == null)
        {
            return Unauthorized();
        }

        int personelId = int.Parse(personelIdMetni);
        PersonelDto profilBilgisi = await personelServis.GetByIdAsync(personelId);
        if (profilBilgisi == null)
        {
            return NotFound();
        }

        return Ok(profilBilgisi);
    }

    [HttpPut("profil/email")]
    [Authorize(Roles = "Calisan")]
    public async Task<IActionResult> UpdateEmail([FromBody] CalisanEmailUpdateDto dto)
    {
        string personelIdMetni = User.FindFirstValue("PersonelId");
        if (personelIdMetni == null)
        {
            return Unauthorized();
        }

        int personelId = int.Parse(personelIdMetni);
        bool islemBasarili = await personelServis.UpdateEmailAsync(personelId, dto);
        if (islemBasarili == false)
        {
            return NotFound();
        }

        return NoContent();
    }
}
