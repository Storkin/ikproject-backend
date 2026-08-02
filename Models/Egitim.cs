namespace IkProjesi.Models;

public class Egitim
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public string Ad { get; set; } = string.Empty;
    public string Kurum { get; set; } = string.Empty;
    public DateTime TamamlanmaTarihi { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string? Aciklama { get; set; }
}
