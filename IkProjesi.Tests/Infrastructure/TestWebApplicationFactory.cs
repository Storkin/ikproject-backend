using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IkProjesi.Tests.Infrastructure;

/// <summary>
/// API'yi bellek icinde ayaga kaldirir ve ayri bir test veritabanina baglar.
/// Uretim veritabanina (IkProjesiDb) asla dokunulmaz.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString =
        "Host=localhost;Port=5432;Database=IkProjesiDb_Test;Username=postgres;Password=1234";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureServices(services =>
        {
            // Uygulamanin kendi DbContext kaydini kaldirip test veritabanina yonlendiriyoruz.
            ServiceDescriptor? contextOptions = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (contextOptions != null)
            {
                services.Remove(contextOptions);
            }

            IEnumerable<ServiceDescriptor> npgsqlRegistrations = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("Npgsql"))
                .ToList();

            foreach (ServiceDescriptor registration in npgsqlRegistrations)
            {
                services.Remove(registration);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(TestConnectionString));
        });
    }

    /// <summary>
    /// Veritabanini sifirdan olusturur ve temel hesaplari yazar.
    /// Her test koleksiyonu basinda bir kez calisir.
    /// </summary>
    public void ResetDatabase()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.EnsureDeleted();
        db.Database.Migrate();

        Seed(db);
    }

    private static void Seed(AppDbContext db)
    {
        Personel calisanPersonel = new Personel
        {
            Ad = "Ahmet",
            Soyad = "Yilmaz",
            Departman = Departman.Muhasebe,
            Unvan = Unvan.Uzman,
            Maas = 27000,
            IseBaslamaTarihi = DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc),
            Email = TestUsers.CalisanEmail,
            AktifMi = true
        };

        Personel meslektas = new Personel
        {
            Ad = "Zeynep",
            Soyad = "Aydin",
            Departman = Departman.Muhasebe,
            Unvan = Unvan.Uzman,
            Maas = 30000,
            IseBaslamaTarihi = DateTime.SpecifyKind(new DateTime(2024, 3, 1), DateTimeKind.Utc),
            Email = TestUsers.MeslektasEmail,
            AktifMi = true
        };

        Personel baskaDepartman = new Personel
        {
            Ad = "Burak",
            Soyad = "Kaya",
            Departman = Departman.BilgiIslem,
            Unvan = Unvan.Muhendis,
            Maas = 40000,
            IseBaslamaTarihi = DateTime.SpecifyKind(new DateTime(2024, 6, 1), DateTimeKind.Utc),
            Email = TestUsers.BaskaDepartmanEmail,
            AktifMi = true
        };

        db.Personeller.AddRange(calisanPersonel, meslektas, baskaDepartman);
        db.SaveChanges();

        db.Users.AddRange(
            new IkYonetici
            {
                Email = TestUsers.IkEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestUsers.IkPassword),
                Rol = "IkYonetici",
                IsFirstLogin = false
            },
            new Admin
            {
                Email = TestUsers.AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestUsers.AdminPassword),
                Rol = "Admin",
                IsFirstLogin = false
            },
            new Calisan
            {
                Email = TestUsers.CalisanEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestUsers.CalisanPassword),
                Rol = "Calisan",
                IsFirstLogin = false,
                PersonelId = calisanPersonel.Id
            },
            new Calisan
            {
                Email = TestUsers.MeslektasEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestUsers.MeslektasPassword),
                Rol = "Calisan",
                IsFirstLogin = false,
                PersonelId = meslektas.Id
            });

        db.SaveChanges();
    }

    public AppDbContext CreateDbContext()
    {
        IServiceScope scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}

public static class TestUsers
{
    public const string IkEmail = "ik@test.com";
    public const string IkPassword = "Ik123!";

    public const string AdminEmail = "admin@test.com";
    public const string AdminPassword = "Admin123!";

    public const string CalisanEmail = "ahmet@test.com";
    public const string CalisanPassword = "Ahmet123!";

    public const string MeslektasEmail = "zeynep@test.com";
    public const string MeslektasPassword = "Zeynep123!";

    // Ayni departmanda olmayan, giris hesabi bulunmayan personel
    public const string BaskaDepartmanEmail = "burak@test.com";
}
