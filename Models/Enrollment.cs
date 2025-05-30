namespace api_school_system.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
} 