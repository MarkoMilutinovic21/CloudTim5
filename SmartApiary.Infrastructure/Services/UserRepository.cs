using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
    }
}