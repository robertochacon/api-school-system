using Microsoft.EntityFrameworkCore;
using api_school_system.Models;

namespace api_school_system.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Using a static hash value for the admin user
        // This hash corresponds to the password "admin123"
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKy7I8rbV5Jk6VK",
            Role = "admin"
        });
    }
} 