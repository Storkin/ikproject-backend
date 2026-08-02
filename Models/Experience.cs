namespace IkProjesi.Models;

public class Experience
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public Personel Personel { get; set; } = null!;
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
}
