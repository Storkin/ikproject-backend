namespace IkProjesi.Models;

public enum MaasKaydiTuru { Maas, Prim, Bonus }

public class MaasKaydi
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public decimal Tutar { get; set; }
    public MaasKaydiTuru Tur { get; set; }
    public DateTime GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}
