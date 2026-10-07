namespace GraduacionUni.Domain.Entities;

public sealed class ProjectDocument
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    /// <summary>Nombre original que subió el estudiante (solo para mostrar).</summary>
    public required string FileName { get; set; }

    /// <summary>Nombre generado por el servidor con el que se guarda el archivo.</summary>
    public required string StoredName { get; set; }

    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}
