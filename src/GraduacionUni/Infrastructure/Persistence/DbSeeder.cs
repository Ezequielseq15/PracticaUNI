using GraduacionUni.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduacionUni.Infrastructure.Persistence;

/// <summary>
/// Datos de demostración SOLO para el entorno Development.
/// La contraseña de demo no protege nada real; en producción nunca se siembran usuarios así.
/// </summary>
public static class DbSeeder
{
    public const string DemoPassword = "Uni2026*Dev";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Si ya ejecutó el script SQL, esto no hace nada; si no, crea las tablas.
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Users.AnyAsync(ct))
            return;

        User NewUser(string email, string role)
        {
            var user = new User { Email = email, PasswordHash = "pending", Role = role };
            user.PasswordHash = hasher.HashPassword(user, DemoPassword);
            return user;
        }

        var student = NewUser("estudiante@uni.edu.ni", Roles.Estudiante);
        var tutor = NewUser("tutor@uni.edu.ni", Roles.Tutor);
        var coordinator = NewUser("coordinador@uni.edu.ni", Roles.Coordinador);
        var admin = NewUser("admin@uni.edu.ni", Roles.Administrador);

        db.Users.AddRange(student, tutor, coordinator, admin);
        await db.SaveChangesAsync(ct);

        db.Projects.Add(new Project
        {
            Title = "Sistema de control de inventario para laboratorios",
            Description = "Propuesta de ejemplo creada por el sembrado de datos.",
            StudentId = student.Id,
            TutorId = tutor.Id
        });
        await db.SaveChangesAsync(ct);
    }
}
