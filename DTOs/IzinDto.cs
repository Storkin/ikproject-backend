using IkProjesi.Models;

namespace IkProjesi.DTOs;

public class IzinTalepOlusturDto
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public IzinTuru Turu { get; set; } = IzinTuru.Yillik;
    public int? SubstituteId { get; set; }
    public string? Aciklama { get; set; }
}

public class SubstituteCandidateDto
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public Unvan Unvan { get; set; }
}

public class IzinOzetDto
{
    public int Yil { get; set; }
    public int HakEdilenGun { get; set; }
    public int DevredenGun { get; set; }
    public int ToplamHak { get; set; }
    public int KullanilanGun { get; set; }
    public int KalanGun { get; set; }
    public int KullanilanMazeretGun { get; set; }
    public int KullanilanUcretsizGun { get; set; }
    public List<IzinTalepDto> Gecmis { get; set; } = new();
}

public class IzinHakkiDto
{
    public int Yil { get; set; }
    public int HakEdilenGun { get; set; }
    public int DevredenGun { get; set; }
    public int ToplamHak { get; set; }
    public int KullanilanGun { get; set; }
    public int KalanGun { get; set; }
    public int KullanilanMazeretGun { get; set; }
    public int KullanilanUcretsizGun { get; set; }
}

public class IzinTalepDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public int? SubstituteId { get; set; }
    public string? SubstituteAdSoyad { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string Turu { get; set; } = string.Empty;
    public string Durum { get; set; } = string.Empty;
    public DateTime TalepTarihi { get; set; }
    public string? Aciklama { get; set; }

    // Talebi degerlendirirken kisinin guncel yillik izin durumu
    public int ToplamHak { get; set; }
    public int KalanGun { get; set; }
}
