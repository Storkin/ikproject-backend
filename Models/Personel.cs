namespace IkProjesi.Models;

public class Personel
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Departman { get; set; } = string.Empty;
    public decimal Maas { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }
    public string Email { get; set; } = string.Empty;
    public int YillikIzinHakki { get; set; } = 14;
    public int KullanılanIzin { get; set; } = 0;
}
