namespace GraduacionUni.Application.Services;

/// <summary>Abstracción del sistema externo "Correo institucional" (C4 Nivel 1 y 2).</summary>
public interface INotificationService
{
    Task NotifyAsync(string toEmail, string subject, string body, CancellationToken ct);
}
