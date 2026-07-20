using IkProjesi.Models;

namespace IkProjesi.DTOs;

public class IzinTalepOlusturDto
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class IzinOzetDto
{
    public int ToplamHak { get; set; }
    public int KullanilanGun { get; set; }
    public int KalanGun { get; set; }
    public List<IzinTalepDto> Gecmis { get; set; } = new();
}

public class IzinTalepDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string Durum { get; set; } = string.Empty;
    public DateTime TalepTarihi { get; set; }
    public string? Aciklama { get; set; }
}
