using GraduacionUni.Application.Services;

namespace GraduacionUni.Infrastructure.Storage;

public sealed class LocalDocumentStorage(IConfiguration configuration, IWebHostEnvironment environment)
    : IDocumentStorage
{
    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
    {
        var relative = configuration["Storage:RootPath"] ?? "storage";
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, relative));
        Directory.CreateDirectory(root);

        // El nombre lo genera el servidor: nunca se usa el nombre que envía el cliente.
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(root, storedName);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, ct);

        return storedName;
    }
}
