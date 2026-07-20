namespace IkProjesi.DTOs;

public class PersonelDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Departman { get; set; } = string.Empty;
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class PersonelCreateDto
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Departman { get; set; } = string.Empty;
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class PersonelUpdateDto
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Departman { get; set; } = string.Empty;
    public decimal Maas { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class CalisanEmailUpdateDto
{
    public string Email { get; set; } = string.Empty;
}
