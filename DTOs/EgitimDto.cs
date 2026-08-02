namespace IkProjesi.DTOs;

public class EgitimOlusturDto
{
    public int PersonelId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kurum { get; set; } = string.Empty;
    public DateTime TamamlanmaTarihi { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class EgitimGuncelleDto
{
    public string Ad { get; set; } = string.Empty;
    public string Kurum { get; set; } = string.Empty;
    public DateTime TamamlanmaTarihi { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class EgitimDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Kurum { get; set; } = string.Empty;
    public DateTime TamamlanmaTarihi { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}
