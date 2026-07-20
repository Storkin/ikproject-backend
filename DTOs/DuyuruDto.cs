namespace IkProjesi.DTOs;

public class DuyuruOlusturDto
{
    public string Baslik { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
}

public class DuyuruDto
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public DateTime YayinTarihi { get; set; }
}
