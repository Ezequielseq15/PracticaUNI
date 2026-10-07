using GraduacionUni.Application.DTOs;

namespace GraduacionUni.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
}
