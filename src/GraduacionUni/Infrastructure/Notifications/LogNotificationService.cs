using GraduacionUni.Application.Services;

namespace GraduacionUni.Infrastructure.Notifications;

/// <summary>
/// Simula el envío de correo escribiendo en el log.
/// Para producción se reemplaza por una implementación SMTP sin tocar la API (misma interfaz).
/// </summary>
public sealed class LogNotificationService(ILogger<LogNotificationService> logger) : INotificationService
{
    public Task NotifyAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        logger.LogInformation("Notificación simulada a {Email}: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
