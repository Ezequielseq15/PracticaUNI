using System.ComponentModel.DataAnnotations;

namespace GraduacionUni.Application.DTOs;

// En un record, los atributos de validación van sobre los parámetros del constructor
// (sin el prefijo "property:"), de lo contrario MVC lanza InvalidOperationException.
public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, StringLength(200)] string Password);

public sealed record LoginResponse(
    int UserId,
    string Email,
    string Role,
    string Token);
