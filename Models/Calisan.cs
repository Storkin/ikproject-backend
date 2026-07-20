namespace IkProjesi.Models;

public class Calisan : User
{
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
}
