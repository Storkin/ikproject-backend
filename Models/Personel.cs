namespace IkProjesi.Models;

public class Personel
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public Departman Departman { get; set; }
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
    public Unvan Unvan { get; set; }
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public string? Iban { get; set; }
    public DateTime? DogumTarihi { get; set; }
}
