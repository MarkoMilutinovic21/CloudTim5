namespace SmartApiary.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default)
        => await context.Users.ToListAsync(ct);

    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
    }
}