namespace IkProjesi.Models;

public class IzinHakki
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public int Yil { get; set; }
    public int HakEdilen { get; set; }
    public int Devreden { get; set; }
    public int Kullanilan { get; set; }
    public int KullanilanMazeret { get; set; }
    public int KullanilanUcretsiz { get; set; }
}
