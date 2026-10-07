using GraduacionUni.Application.DTOs;

namespace GraduacionUni.Application.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectSummaryResponse>> GetMineAsync(CurrentUser user, CancellationToken ct);
    Task<ServiceResult<ProjectDetailResponse>> GetByIdAsync(int id, CurrentUser user, CancellationToken ct);
    Task<ServiceResult<ProjectDetailResponse>> CreateAsync(CreateProjectRequest request, CurrentUser user, CancellationToken ct);
    Task<ServiceResult<ReviewResponse>> AddReviewAsync(int projectId, CreateReviewRequest request, CurrentUser user, CancellationToken ct);
    Task<ServiceResult<ProjectSummaryResponse>> ChangeStatusAsync(int id, ChangeStatusRequest request, CancellationToken ct);
    Task<ServiceResult<ProjectSummaryResponse>> AssignTutorAsync(int id, AssignTutorRequest request, CancellationToken ct);
    Task<ServiceResult<DocumentResponse>> UploadDocumentAsync(
        int projectId, string fileName, string contentType, long length, Stream content,
        CurrentUser user, CancellationToken ct);
}
