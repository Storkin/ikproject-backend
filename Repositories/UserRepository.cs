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
        User found = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return found;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        User found = await db.Users.FindAsync(id);
        return found;
    }

    public async Task AddAsync(User user)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
