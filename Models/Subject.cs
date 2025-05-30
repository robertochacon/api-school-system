namespace api_school_system.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
} 