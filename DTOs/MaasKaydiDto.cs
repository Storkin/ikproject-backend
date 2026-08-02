using IkProjesi.Models;

namespace IkProjesi.DTOs;

public class MaasKaydiOlusturDto
{
    public int PersonelId { get; set; }
    public decimal Tutar { get; set; }
    public MaasKaydiTuru Tur { get; set; }
    public DateTime GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class MaasKaydiDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public string Tur { get; set; } = string.Empty;
    public DateTime GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}
