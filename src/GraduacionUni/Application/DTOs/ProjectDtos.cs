using System.ComponentModel.DataAnnotations;

namespace GraduacionUni.Application.DTOs;

// ----- Requests -----
public sealed class CreateProjectRequest
{
    [Required, StringLength(250, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }
}

public sealed class CreateReviewRequest
{
    [Required, StringLength(2000, MinimumLength = 3)]
    public string Comments { get; set; } = string.Empty;
}

public sealed class ChangeStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public sealed class AssignTutorRequest
{
    [Range(1, int.MaxValue)]
    public int TutorId { get; set; }
}

// ----- Responses -----
public sealed record ProjectSummaryResponse(int Id, string Title, string Status);

public sealed record ReviewResponse(int Id, string TutorEmail, string Comments, DateTime CreatedAt);

public sealed record DocumentResponse(int Id, string FileName, string ContentType, long SizeBytes, DateTime UploadedAt);

public sealed record ProjectDetailResponse(
    int Id,
    string Title,
    string? Description,
    string Status,
    int StudentId,
    int? TutorId,
    DateTime CreatedAt,
    IReadOnlyList<ReviewResponse> Reviews,
    IReadOnlyList<DocumentResponse> Documents);
