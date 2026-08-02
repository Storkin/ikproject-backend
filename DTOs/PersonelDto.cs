using IkProjesi.Models;

namespace IkProjesi.DTOs;

public class PersonelDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public Departman Departman { get; set; }
    public Unvan Unvan { get; set; }
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public string? Iban { get; set; }
    public DateTime? DogumTarihi { get; set; }
    public bool AktifMi { get; set; }
    public DateTime? IseCikisTarihi { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = new();
}

public class PersonelCreateDto
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public Departman Departman { get; set; }
    public Unvan Unvan { get; set; }
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public string? Iban { get; set; }
    public DateTime? DogumTarihi { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = new();
}

public class PersonelUpdateDto
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public Departman Departman { get; set; }
    public Unvan Unvan { get; set; }
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public string? Iban { get; set; }
    public DateTime? DogumTarihi { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = new();
}

public class CalisanProfilUpdateDto
{
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public string? Iban { get; set; }
}
