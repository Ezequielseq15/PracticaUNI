namespace GraduacionUni.Application.Services;

/// <summary>
/// Abstracción del contenedor "Gestor de Documentos" del C4 Nivel 2.
/// Hoy guarda en disco; mañana puede ser Azure Blob o S3 sin tocar la API.
/// </summary>
public interface IDocumentStorage
{
    /// <returns>Nombre con el que quedó almacenado el archivo.</returns>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);
}
