namespace GraduacionUni.Domain.Entities;

public sealed class Project
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = ProjectStatuses.Propuesta;
    public int StudentId { get; set; }
    public int? TutorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Student { get; set; }
    public User? Tutor { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}
