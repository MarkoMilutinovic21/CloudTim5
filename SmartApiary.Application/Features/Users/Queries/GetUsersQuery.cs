namespace SmartApiary.Application.Features.Users.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetUsersQuery : IRequest<IReadOnlyCollection<UserDto>>;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    string? ActivationToken);

public class GetUsersQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var users = await userRepository.GetAllAsync(ct);
        return users.Select(u => new UserDto(
            u.Id, u.FirstName, u.LastName, u.Email,
            u.Phone, u.Role, u.IsActive, u.CreatedAt,
            u.ActivationToken))
            .ToList()
            .AsReadOnly();
    }
}