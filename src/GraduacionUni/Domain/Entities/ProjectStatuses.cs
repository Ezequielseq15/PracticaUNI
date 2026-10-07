namespace GraduacionUni.Domain.Entities;

/// <summary>
/// Estados del proyecto (sección 4.1 de la guía) y transiciones permitidas.
/// </summary>
public static class ProjectStatuses
{
    public const string Propuesta = "Propuesta";
    public const string Revision = "Revision";
    public const string Aprobada = "Aprobada";
    public const string EnDesarrollo = "EnDesarrollo";
    public const string Defensa = "Defensa";
    public const string Cerrada = "Cerrada";

    public static readonly string[] All =
        [Propuesta, Revision, Aprobada, EnDesarrollo, Defensa, Cerrada];

    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        [Propuesta] = [Revision],
        [Revision] = [Propuesta, Aprobada],   // puede devolverse a Propuesta para correcciones
        [Aprobada] = [EnDesarrollo],
        [EnDesarrollo] = [Defensa],
        [Defensa] = [Cerrada],
        [Cerrada] = []
    };

    public static string? Normalize(string? value) =>
        All.FirstOrDefault(s => string.Equals(s, value?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool CanTransition(string from, string to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
