namespace GraduacionUni.Application.Services;

/// <summary>Datos del usuario autenticado extraídos del token JWT.</summary>
public sealed record CurrentUser(int Id, string Email, string Role);
