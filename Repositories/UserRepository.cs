using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext db;

    public UserRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        User bulunanKullanici = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return bulunanKullanici;
    }

    public async Task AddAsync(User kullanici)
    {
        await db.Users.AddAsync(kullanici);
        await db.SaveChangesAsync();
    }
}
