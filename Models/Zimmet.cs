namespace IkProjesi.Models;

public class Zimmet
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public string EsyaAdi { get; set; } = string.Empty;
    public string? SeriNo { get; set; }
    public DateTime ZimmetTarihi { get; set; }
    public DateTime? IadeTarihi { get; set; }
    public string? Aciklama { get; set; }
}
