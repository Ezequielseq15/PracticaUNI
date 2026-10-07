using GraduacionUni.Api.Extensions;
using GraduacionUni.Application.DTOs;
using GraduacionUni.Application.Services;
using GraduacionUni.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraduacionUni.Api.Controllers;

[Route("api/projects")]
[Authorize]
public sealed class ProjectsController(IProjectService projects) : ApiControllerBase
{
    // GET /api/projects/mine
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var current = User.ToCurrentUser();
        if (current is null) return Unauthorized();

        return Ok(await projects.GetMineAsync(current, ct));
    }

    // GET /api/projects/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var current = User.ToCurrentUser();
        if (current is null) return Unauthorized();

        var result = await projects.GetByIdAsync(id, current, ct);
        return result.Succeeded ? Ok(result.Value) : ToErrorResult(result);
    }

    // POST /api/projects
    [HttpPost]
    [Authorize(Roles = Roles.Estudiante)]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var current = User.ToCurrentUser();
        if (current is null) return Unauthorized();

        var result = await projects.CreateAsync(request, current, ct);
        if (!result.Succeeded) return ToErrorResult(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    // POST /api/projects/{id}/reviews
    [HttpPost("{id:int}/reviews")]
    [Authorize(Roles = Roles.Tutor)]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReview(int id, CreateReviewRequest request, CancellationToken ct)
    {
        var current = User.ToCurrentUser();
        if (current is null) return Unauthorized();

        var result = await projects.AddReviewAsync(id, request, current, ct);
        if (!result.Succeeded) return ToErrorResult(result);

        return Created($"/api/projects/{id}", result.Value);
    }

    // PATCH /api/projects/{id}/status
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = $"{Roles.Coordinador},{Roles.Administrador}")]
    [ProducesResponseType(typeof(ProjectSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await projects.ChangeStatusAsync(id, request, ct);
        return result.Succeeded ? Ok(result.Value) : ToErrorResult(result);
    }

    // PATCH /api/projects/{id}/tutor
    [HttpPatch("{id:int}/tutor")]
    [Authorize(Roles = $"{Roles.Coordinador},{Roles.Administrador}")]
    [ProducesResponseType(typeof(ProjectSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTutor(int id, AssignTutorRequest request, CancellationToken ct)
    {
        var result = await projects.AssignTutorAsync(id, request, ct);
        return result.Succeeded ? Ok(result.Value) : ToErrorResult(result);
    }

    // POST /api/projects/{id}/documents
    [HttpPost("{id:int}/documents")]
    [Authorize(Roles = Roles.Estudiante)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file, CancellationToken ct)
    {
        var current = User.ToCurrentUser();
        if (current is null) return Unauthorized();

        await using var stream = file.OpenReadStream();
        var result = await projects.UploadDocumentAsync(
            id, file.FileName, file.ContentType, file.Length, stream, current, ct);

        if (!result.Succeeded) return ToErrorResult(result);

        return Created($"/api/projects/{id}", result.Value);
    }
}
