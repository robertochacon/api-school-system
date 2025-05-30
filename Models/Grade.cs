namespace api_school_system.Models;

public class Grade
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public float Score { get; set; }
} 