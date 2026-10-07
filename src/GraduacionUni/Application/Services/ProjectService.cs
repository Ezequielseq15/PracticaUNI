using GraduacionUni.Application.DTOs;
using GraduacionUni.Domain.Entities;
using GraduacionUni.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GraduacionUni.Application.Services;

public sealed class ProjectService(
    AppDbContext db,
    IDocumentStorage storage,
    INotificationService notifications) : IProjectService
{
    private const long MaxDocumentBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".zip"];

    // ---------- GET /api/projects/mine ----------
    public async Task<IReadOnlyList<ProjectSummaryResponse>> GetMineAsync(CurrentUser user, CancellationToken ct)
    {
        IQueryable<Project> query = db.Projects.AsNoTracking();

        query = user.Role switch
        {
            Roles.Estudiante => query.Where(p => p.StudentId == user.Id),
            Roles.Tutor => query.Where(p => p.TutorId == user.Id),
            _ => query // Coordinador y Administrador ven todos
        };

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectSummaryResponse(p.Id, p.Title, p.Status))
            .ToListAsync(ct);
    }

    // ---------- GET /api/projects/{id} ----------
    public async Task<ServiceResult<ProjectDetailResponse>> GetByIdAsync(int id, CurrentUser user, CancellationToken ct)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Reviews).ThenInclude(r => r.Tutor)
            .Include(p => p.Documents)
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
            return ServiceResult<ProjectDetailResponse>.Fail(ServiceError.NotFound, "Proyecto no encontrado.");

        if (!CanAccess(project, user))
            return ServiceResult<ProjectDetailResponse>.Fail(ServiceError.Forbidden, "No tiene acceso a este proyecto.");

        return ServiceResult<ProjectDetailResponse>.Ok(ToDetail(project));
    }

    // ---------- POST /api/projects ----------
    public async Task<ServiceResult<ProjectDetailResponse>> CreateAsync(
        CreateProjectRequest request, CurrentUser user, CancellationToken ct)
    {
        var title = request.Title.Trim();
        if (title.Length < 5)
            return ServiceResult<ProjectDetailResponse>.Fail(ServiceError.Invalid, "El título debe tener al menos 5 caracteres.");

        var project = new Project
        {
            Title = title,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            StudentId = user.Id
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);

        return ServiceResult<ProjectDetailResponse>.Ok(ToDetail(project));
    }

    // ---------- POST /api/projects/{id}/reviews ----------
    public async Task<ServiceResult<ReviewResponse>> AddReviewAsync(
        int projectId, CreateReviewRequest request, CurrentUser user, CancellationToken ct)
    {
        var project = await db.Projects
            .Include(p => p.Student)
            .SingleOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return ServiceResult<ReviewResponse>.Fail(ServiceError.NotFound, "Proyecto no encontrado.");

        if (project.TutorId != user.Id)
            return ServiceResult<ReviewResponse>.Fail(ServiceError.Forbidden, "Solo el tutor asignado puede registrar revisiones.");

        var review = new Review
        {
            ProjectId = project.Id,
            TutorId = user.Id,
            Comments = request.Comments.Trim()
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);

        if (project.Student is not null)
        {
            await notifications.NotifyAsync(
                project.Student.Email,
                "Nueva revisión de su proyecto",
                $"Su tutor registró observaciones en el proyecto «{project.Title}».",
                ct);
        }

        return ServiceResult<ReviewResponse>.Ok(
            new ReviewResponse(review.Id, user.Email, review.Comments, review.CreatedAt));
    }

    // ---------- PATCH /api/projects/{id}/status ----------
    public async Task<ServiceResult<ProjectSummaryResponse>> ChangeStatusAsync(
        int id, ChangeStatusRequest request, CancellationToken ct)
    {
        var newStatus = ProjectStatuses.Normalize(request.Status);
        if (newStatus is null)
            return ServiceResult<ProjectSummaryResponse>.Fail(
                ServiceError.Invalid,
                $"Estado inválido. Valores permitidos: {string.Join(", ", ProjectStatuses.All)}.");

        var project = await db.Projects
            .Include(p => p.Student)
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
            return ServiceResult<ProjectSummaryResponse>.Fail(ServiceError.NotFound, "Proyecto no encontrado.");

        if (!ProjectStatuses.CanTransition(project.Status, newStatus))
            return ServiceResult<ProjectSummaryResponse>.Fail(
                ServiceError.Invalid,
                $"No se puede pasar de «{project.Status}» a «{newStatus}».");

        project.Status = newStatus;
        await db.SaveChangesAsync(ct);

        if (project.Student is not null)
        {
            await notifications.NotifyAsync(
                project.Student.Email,
                "Cambió el estado de su proyecto",
                $"El proyecto «{project.Title}» ahora está en estado: {newStatus}.",
                ct);
        }

        return ServiceResult<ProjectSummaryResponse>.Ok(
            new ProjectSummaryResponse(project.Id, project.Title, project.Status));
    }

    // ---------- PATCH /api/projects/{id}/tutor ----------
    public async Task<ServiceResult<ProjectSummaryResponse>> AssignTutorAsync(
        int id, AssignTutorRequest request, CancellationToken ct)
    {
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
            return ServiceResult<ProjectSummaryResponse>.Fail(ServiceError.NotFound, "Proyecto no encontrado.");

        var tutor = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.TutorId && u.Role == Roles.Tutor, ct);

        if (tutor is null)
            return ServiceResult<ProjectSummaryResponse>.Fail(ServiceError.Invalid, "El tutor indicado no existe.");

        project.TutorId = tutor.Id;
        await db.SaveChangesAsync(ct);

        await notifications.NotifyAsync(
            tutor.Email,
            "Se le asignó un proyecto",
            $"Usted fue asignado como tutor del proyecto «{project.Title}».",
            ct);

        return ServiceResult<ProjectSummaryResponse>.Ok(
            new ProjectSummaryResponse(project.Id, project.Title, project.Status));
    }

    // ---------- POST /api/projects/{id}/documents ----------
    public async Task<ServiceResult<DocumentResponse>> UploadDocumentAsync(
        int projectId, string fileName, string contentType, long length, Stream content,
        CurrentUser user, CancellationToken ct)
    {
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null)
            return ServiceResult<DocumentResponse>.Fail(ServiceError.NotFound, "Proyecto no encontrado.");

        if (project.StudentId != user.Id)
            return ServiceResult<DocumentResponse>.Fail(ServiceError.Forbidden, "Solo el estudiante autor puede adjuntar documentos.");

        if (length <= 0)
            return ServiceResult<DocumentResponse>.Fail(ServiceError.Invalid, "El archivo está vacío.");

        if (length > MaxDocumentBytes)
            return ServiceResult<DocumentResponse>.Fail(ServiceError.Invalid, "El archivo supera el máximo de 5 MB.");

        // Path.GetFileName elimina cualquier ruta que venga en el nombre (evita path traversal).
        var safeName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return ServiceResult<DocumentResponse>.Fail(
                ServiceError.Invalid,
                $"Extensión no permitida. Use: {string.Join(", ", AllowedExtensions)}.");

        var storedName = await storage.SaveAsync(content, extension, ct);

        var document = new ProjectDocument
        {
            ProjectId = project.Id,
            FileName = safeName.Length > 260 ? safeName[..260] : safeName,
            StoredName = storedName,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            SizeBytes = length
        };

        db.ProjectDocuments.Add(document);
        await db.SaveChangesAsync(ct);

        return ServiceResult<DocumentResponse>.Ok(
            new DocumentResponse(document.Id, document.FileName, document.ContentType, document.SizeBytes, document.UploadedAt));
    }

    // ---------- helpers ----------
    private static bool CanAccess(Project project, CurrentUser user) => user.Role switch
    {
        Roles.Coordinador or Roles.Administrador => true,
        Roles.Estudiante => project.StudentId == user.Id,
        Roles.Tutor => project.TutorId == user.Id,
        _ => false
    };

    private static ProjectDetailResponse ToDetail(Project p) => new(
        p.Id,
        p.Title,
        p.Description,
        p.Status,
        p.StudentId,
        p.TutorId,
        p.CreatedAt,
        p.Reviews
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ReviewResponse(r.Id, r.Tutor?.Email ?? string.Empty, r.Comments, r.CreatedAt))
            .ToList(),
        p.Documents
            .OrderBy(d => d.UploadedAt)
            .Select(d => new DocumentResponse(d.Id, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt))
            .ToList());
}
