namespace GraduacionUni.Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }

    /// <summary>Proyectos donde el usuario es el estudiante autor.</summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    /// <summary>Proyectos donde el usuario es el tutor asignado.</summary>
    public ICollection<Project> TutoredProjects { get; set; } = new List<Project>();
}
