namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    Task<User?> GetByActivationTokenAsync(string token, CancellationToken ct = default);
    Task<User?> GetByResetPasswordTokenAsync(string token, CancellationToken ct = default);
}