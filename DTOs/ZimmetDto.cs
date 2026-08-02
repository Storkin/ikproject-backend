namespace IkProjesi.DTOs;

public class ZimmetOlusturDto
{
    public int PersonelId { get; set; }
    public string EsyaAdi { get; set; } = string.Empty;
    public string? SeriNo { get; set; }
    public DateTime ZimmetTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class ZimmetDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public string EsyaAdi { get; set; } = string.Empty;
    public string? SeriNo { get; set; }
    public DateTime ZimmetTarihi { get; set; }
    public DateTime? IadeTarihi { get; set; }
    public string? Aciklama { get; set; }
}
