namespace GraduacionUni.Domain.Entities;

public sealed class Review
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int TutorId { get; set; }
    public required string Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public User? Tutor { get; set; }
}
