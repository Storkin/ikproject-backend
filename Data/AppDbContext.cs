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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("Rol")
            .HasValue<Admin>("Admin")
            .HasValue<IkYonetici>("IkYonetici")
            .HasValue<Calisan>("Calisan");
    }
}
