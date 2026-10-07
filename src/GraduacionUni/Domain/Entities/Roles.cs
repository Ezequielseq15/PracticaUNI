namespace GraduacionUni.Domain.Entities;

public static class Roles
{
    public const string Estudiante = "Estudiante";
    public const string Tutor = "Tutor";
    public const string Coordinador = "Coordinador";
    public const string Administrador = "Administrador";

    public static readonly string[] All = [Estudiante, Tutor, Coordinador, Administrador];
}
