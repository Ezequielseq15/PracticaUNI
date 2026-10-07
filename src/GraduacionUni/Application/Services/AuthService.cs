using GraduacionUni.Application.DTOs;
using GraduacionUni.Domain.Entities;
using GraduacionUni.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduacionUni.Application.Services;

public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email == email, ct);

        // Mismo resultado (null -> 401) si el usuario no existe o la clave es incorrecta:
        // no se revela cuál de las dos falló.
        if (user is null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        var token = tokenService.CreateToken(user);

        return new LoginResponse(user.Id, user.Email, user.Role, token);
    }
}
