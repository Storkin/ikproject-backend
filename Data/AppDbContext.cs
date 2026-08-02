using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Personel> Personeller { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<IzinTalep> IzinTalepler { get; set; }
    public DbSet<Duyuru> Duyurular { get; set; }
    public DbSet<IzinHakki> IzinHaklari { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<Zimmet> Zimmetler { get; set; }
    public DbSet<Egitim> Egitimler { get; set; }
    public DbSet<MaasKaydi> MaasKayitlari { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("Rol")
            .HasValue<Admin>("Admin")
            .HasValue<IkYonetici>("IkYonetici")
            .HasValue<Calisan>("Calisan");

        // IzinTalep'in Personel'e iki ayri bagi var: talebi acan ve yerine bakacak kisi.
        // Talep sahibi silinince talep de silinir; yerine bakan kisi silinirse
        // talep durur, sadece alan bosalir.
        modelBuilder.Entity<IzinTalep>()
            .HasOne(t => t.Personel)
            .WithMany()
            .HasForeignKey(t => t.PersonelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IzinTalep>()
            .HasOne(t => t.Substitute)
            .WithMany()
            .HasForeignKey(t => t.SubstituteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
