namespace IkProjesi.Models;

public enum IzinDurum { Beklemede, Onaylandi, Reddedildi }

public enum IzinTuru { Yillik, Mazeret, Hastalik, Ucretsiz }

public class IzinTalep
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public int? SubstituteId { get; set; }
    public Personel? Substitute { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public IzinTuru Turu { get; set; } = IzinTuru.Yillik;
    public IzinDurum Durum { get; set; } = IzinDurum.Beklemede;
    public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;
    public string? Aciklama { get; set; }
}
