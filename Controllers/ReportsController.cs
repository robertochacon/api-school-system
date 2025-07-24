using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;
using System.ComponentModel.DataAnnotations;

namespace api_school_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/student-performance
        [HttpGet("student-performance")]
        public async Task<ActionResult<object>> GetStudentPerformanceReport(
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null,
            [FromQuery] int? subjectId = null,
            [FromQuery] int? academicPeriodId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.StudentGrades
                .Include(sg => sg.Student)
                    .ThenInclude(s => s.User)
                .Include(sg => sg.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(sg => sg.Evaluation)
                    .ThenInclude(e => e.Subject)
                .Include(sg => sg.Teacher)
                    .ThenInclude(t => t.User)
                .Where(sg => sg.IsActive);

            if (studentId.HasValue)
                query = query.Where(sg => sg.StudentId == studentId);
            if (courseId.HasValue)
                query = query.Where(sg => sg.Enrollment.CourseId == courseId);
            if (subjectId.HasValue)
                query = query.Where(sg => sg.Evaluation.SubjectId == subjectId);
            if (academicPeriodId.HasValue)
                query = query.Where(sg => sg.Evaluation.AcademicPeriodId == academicPeriodId);
            if (startDate.HasValue)
                query = query.Where(sg => sg.GradedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(sg => sg.GradedAt <= endDate.Value);

            var grades = await query.ToListAsync();

            var report = grades
                .GroupBy(sg => new { sg.StudentId, sg.Student.User.FirstName, sg.Student.User.LastName })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId,
                    StudentName = $"{g.Key.FirstName} {g.Key.LastName}",
                    StudentCode = g.Key.StudentId,
                    TotalEvaluations = g.Count(),
                    AverageScore = Math.Round(g.Average(sg => sg.Score), 2),
                    HighestScore = g.Max(sg => sg.Score),
                    LowestScore = g.Min(sg => sg.Score),
                    PassedEvaluations = g.Count(sg => sg.Score >= 60),
                    FailedEvaluations = g.Count(sg => sg.Score < 60),
                    PassRate = Math.Round((double)g.Count(sg => sg.Score >= 60) / g.Count() * 100, 2),
                    Evaluations = g.Select(sg => new
                    {
                        sg.Id,
                        sg.Score,
                        sg.GradedAt,
                        sg.Comments,
                        Evaluation = new
                        {
                            sg.Evaluation.Name,
                            sg.Evaluation.Type,
                            sg.Evaluation.Weight,
                            sg.Evaluation.MaxScore
                        },
                        Subject = new
                        {
                            sg.Evaluation.Subject.Name,
                            sg.Evaluation.Subject.Code
                        },
                        Course = new
                        {
                            sg.Enrollment.Course.Name,
                            sg.Enrollment.Course.Section
                        },
                        Teacher = new
                        {
                            sg.Teacher.User.FirstName,
                            sg.Teacher.User.LastName
                        }
                    }).OrderByDescending(e => e.GradedAt).ToList()
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            var summary = new
            {
                TotalStudents = report.Count,
                AverageScore = report.Any() ? Math.Round(report.Average(x => x.AverageScore), 2) : 0,
                StudentsWithPerfectScore = report.Count(x => x.AverageScore == 100),
                StudentsBelow60 = report.Count(x => x.AverageScore < 60),
                TopPerformers = report.Take(5).ToList(),
                BottomPerformers = report.OrderBy(x => x.AverageScore).Take(5).ToList()
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/reports/attendance-summary
        [HttpGet("attendance-summary")]
        public async Task<ActionResult<object>> GetAttendanceSummaryReport(
            [FromQuery] int? courseId = null,
            [FromQuery] int? academicPeriodId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.IsActive);

            if (courseId.HasValue)
                query = query.Where(a => a.Enrollment.CourseId == courseId);
            if (academicPeriodId.HasValue)
                query = query.Where(a => a.Enrollment.AcademicPeriodId == academicPeriodId);
            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var attendance = await query.ToListAsync();

            var report = attendance
                .GroupBy(a => new { a.StudentId, a.Student.User.FirstName, a.Student.User.LastName })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId,
                    StudentName = $"{g.Key.FirstName} {g.Key.LastName}",
                    StudentCode = g.Key.StudentId,
                    TotalDays = g.Count(),
                    Present = g.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = g.Count(a => a.Status == AttendanceStatus.Absent),
                    Late = g.Count(a => a.Status == AttendanceStatus.Late),
                    Excused = g.Count(a => a.Status == AttendanceStatus.Excused),
                    Tardy = g.Count(a => a.Status == AttendanceStatus.Tardy),
                    AttendanceRate = Math.Round((double)g.Count(a => a.Status == AttendanceStatus.Present) / g.Count() * 100, 2)
                })
                .OrderBy(x => x.StudentName)
                .ToList();

            var summary = new
            {
                TotalStudents = report.Count,
                AverageAttendanceRate = report.Any() ? Math.Round(report.Average(x => x.AttendanceRate), 2) : 0,
                StudentsWithPerfectAttendance = report.Count(x => x.AttendanceRate == 100),
                StudentsBelow80Percent = report.Count(x => x.AttendanceRate < 80)
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/reports/enrollment-statistics
        [HttpGet("enrollment-statistics")]
        public async Task<ActionResult<object>> GetEnrollmentStatisticsReport(
            [FromQuery] int? academicPeriodId = null,
            [FromQuery] int? gradeId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Grade)
                .Include(e => e.Course)
                .Where(e => e.IsActive);

            if (academicPeriodId.HasValue)
                query = query.Where(e => e.AcademicPeriodId == academicPeriodId);
            if (gradeId.HasValue)
                query = query.Where(e => e.GradeId == gradeId);
            if (startDate.HasValue)
                query = query.Where(e => e.EnrollmentDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.EnrollmentDate <= endDate.Value);

            var enrollments = await query.ToListAsync();

            var report = enrollments
                .GroupBy(e => new { e.GradeId, e.Grade.Name })
                .Select(g => new
                {
                    GradeId = g.Key.GradeId,
                    GradeName = g.Key.Name,
                    TotalEnrollments = g.Count(),
                    ActiveEnrollments = g.Count(e => e.Status == EnrollmentStatus.Active),
                    PendingEnrollments = g.Count(e => e.Status == EnrollmentStatus.Pending),
                    CancelledEnrollments = g.Count(e => e.Status == EnrollmentStatus.Cancelled),
                    AverageEnrollmentDate = new DateTime((long)g.Average(e => e.EnrollmentDate.Ticks))
                })
                .OrderBy(x => x.GradeName)
                .ToList();

            var summary = new
            {
                TotalEnrollments = enrollments.Count,
                ActiveEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                PendingEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Pending),
                CancelledEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Cancelled),
                EnrollmentRate = enrollments.Any() ? Math.Round((double)enrollments.Count(e => e.Status == EnrollmentStatus.Active) / enrollments.Count * 100, 2) : 0
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/reports/teacher-workload
        [HttpGet("teacher-workload")]
        public async Task<ActionResult<object>> GetTeacherWorkloadReport(
            [FromQuery] int? teacherId = null,
            [FromQuery] int? academicPeriodId = null)
        {
            var query = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.CourseTeachers)
                    .ThenInclude(ct => ct.Course)
                .Include(t => t.SubjectTeachers)
                    .ThenInclude(st => st.Subject)
                .Include(t => t.Schedules)
                .Where(t => t.IsActive);

            if (teacherId.HasValue)
                query = query.Where(t => t.Id == teacherId);

            var teachers = await query.ToListAsync();

            var report = teachers.Select(t => new
            {
                TeacherId = t.Id,
                TeacherName = $"{t.User.FirstName} {t.User.LastName}",
                EmployeeId = t.EmployeeId,
                Specialization = t.Specialization,
                TotalCourses = t.CourseTeachers.Count(ct => ct.IsActive),
                TotalSubjects = t.SubjectTeachers.Count(st => st.IsActive),
                TotalSchedules = t.Schedules.Count(s => s.IsActive),
                WeeklyHours = t.Schedules.Where(s => s.IsActive)
                    .Sum(s => (s.EndTime - s.StartTime).TotalHours),
                Courses = t.CourseTeachers.Where(ct => ct.IsActive).Select(ct => new
                {
                    ct.Course.Name,
                    ct.Course.Section,
                    ct.Role
                }).ToList(),
                Subjects = t.SubjectTeachers.Where(st => st.IsActive).Select(st => new
                {
                    st.Subject.Name,
                    st.Subject.Code,
                    st.Subject.Credits
                }).ToList()
            }).ToList();

            var summary = new
            {
                TotalTeachers = report.Count,
                AverageCoursesPerTeacher = report.Any() ? Math.Round(report.Average(x => x.TotalCourses), 2) : 0,
                AverageSubjectsPerTeacher = report.Any() ? Math.Round(report.Average(x => x.TotalSubjects), 2) : 0,
                AverageWeeklyHours = report.Any() ? Math.Round(report.Average(x => x.WeeklyHours), 2) : 0,
                TeachersWithHighWorkload = report.Count(x => x.WeeklyHours > 20)
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/reports/academic-progress
        [HttpGet("academic-progress")]
        public async Task<ActionResult<object>> GetAcademicProgressReport(
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null,
            [FromQuery] int? academicPeriodId = null)
        {
            var query = _context.StudentGrades
                .Include(sg => sg.Student)
                    .ThenInclude(s => s.User)
                .Include(sg => sg.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(sg => sg.Evaluation)
                    .ThenInclude(e => e.Subject)
                .Where(sg => sg.IsActive);

            if (studentId.HasValue)
                query = query.Where(sg => sg.StudentId == studentId);
            if (courseId.HasValue)
                query = query.Where(sg => sg.Enrollment.CourseId == courseId);
            if (academicPeriodId.HasValue)
                query = query.Where(sg => sg.Evaluation.AcademicPeriodId == academicPeriodId);

            var grades = await query.ToListAsync();

            var report = grades
                .GroupBy(sg => new { sg.StudentId, sg.Student.User.FirstName, sg.Student.User.LastName })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId,
                    StudentName = $"{g.Key.FirstName} {g.Key.LastName}",
                    StudentCode = g.Key.StudentId,
                    TotalEvaluations = g.Count(),
                    AverageScore = Math.Round(g.Average(sg => sg.Score), 2),
                    HighestScore = g.Max(sg => sg.Score),
                    LowestScore = g.Min(sg => sg.Score),
                    PassedEvaluations = g.Count(sg => sg.Score >= 60),
                    FailedEvaluations = g.Count(sg => sg.Score < 60),
                    PassRate = Math.Round((double)g.Count(sg => sg.Score >= 60) / g.Count() * 100, 2),
                    Evaluations = g.Select(sg => new
                    {
                        sg.Id,
                        sg.Score,
                        sg.GradedAt,
                        sg.Comments,
                        Evaluation = new
                        {
                            sg.Evaluation.Name,
                            sg.Evaluation.Type,
                            sg.Evaluation.Weight,
                            sg.Evaluation.MaxScore
                        },
                        Subject = new
                        {
                            sg.Evaluation.Subject.Name,
                            sg.Evaluation.Subject.Code
                        },
                        Course = new
                        {
                            sg.Enrollment.Course.Name,
                            sg.Enrollment.Course.Section
                        }
                    }).OrderByDescending(e => e.GradedAt).ToList()
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            var summary = new
            {
                TotalStudents = report.Count,
                AverageScore = report.Any() ? Math.Round(report.Average(x => x.AverageScore), 2) : 0,
                StudentsWithPerfectScore = report.Count(x => x.AverageScore == 100),
                StudentsBelow60 = report.Count(x => x.AverageScore < 60),
                StudentsImproving = report.Count(x => x.Evaluations.Count > 1 && 
                    x.Evaluations.OrderByDescending(e => e.GradedAt).First().Score > 
                    x.Evaluations.OrderByDescending(e => e.GradedAt).Skip(1).First().Score),
                StudentsDeclining = report.Count(x => x.Evaluations.Count > 1 && 
                    x.Evaluations.OrderByDescending(e => e.GradedAt).First().Score < 
                    x.Evaluations.OrderByDescending(e => e.GradedAt).Skip(1).First().Score)
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/reports/dashboard-summary
        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<object>> GetDashboardSummary()
        {
            var currentDate = DateTime.Today;

            // Estadísticas generales
            var totalStudents = await _context.Students.CountAsync(s => s.IsActive);
            var totalTeachers = await _context.Teachers.CountAsync(t => t.IsActive);
            var totalCourses = await _context.Courses.CountAsync(c => c.IsActive);
            var totalSubjects = await _context.Subjects.CountAsync(s => s.IsActive);

            // Período académico actual
            var currentPeriod = await _context.AcademicPeriods
                .Where(p => p.IsActive && p.StartDate <= currentDate && p.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            // Matrículas activas
            var activeEnrollments = await _context.Enrollments
                .CountAsync(e => e.IsActive && e.Status == EnrollmentStatus.Active);

            // Asistencia de hoy
            var todayAttendance = await _context.Attendances
                .CountAsync(a => a.IsActive && a.Date == currentDate);

            // Evaluaciones pendientes
            var pendingEvaluations = await _context.Evaluations
                .CountAsync(e => e.IsActive && e.EvaluationDate > currentDate);

            // Eventos de hoy
            var todayEvents = await _context.AcademicEvents
                .CountAsync(e => e.IsActive && e.StartDate <= currentDate && e.EndDate >= currentDate);

            // Notificaciones no leídas
            var unreadNotifications = await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Unread && n.IsActive);

            // Estadísticas por grado
            var enrollmentsByGrade = await _context.Enrollments
                .Include(e => e.Grade)
                .Where(e => e.IsActive && e.Status == EnrollmentStatus.Active)
                .GroupBy(e => new { e.GradeId, e.Grade.Name })
                .Select(g => new
                {
                    GradeId = g.Key.GradeId,
                    GradeName = g.Key.Name,
                    EnrollmentCount = g.Count()
                })
                .OrderBy(x => x.GradeName)
                .ToListAsync();

            // Rendimiento reciente
            var recentGrades = await _context.StudentGrades
                .Where(sg => sg.IsActive && sg.GradedAt >= currentDate.AddDays(-30))
                .ToListAsync();

            var averageRecentScore = recentGrades.Any() ? 
                Math.Round(recentGrades.Average(sg => sg.Score), 2) : 0;

            return Ok(new
            {
                GeneralStats = new
                {
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalCourses = totalCourses,
                    TotalSubjects = totalSubjects,
                    ActiveEnrollments = activeEnrollments
                },
                CurrentPeriod = currentPeriod != null ? new
                {
                    currentPeriod.Id,
                    currentPeriod.Name,
                    currentPeriod.Code,
                    DaysRemaining = (currentPeriod.EndDate - currentDate).Days
                } : null,
                TodayStats = new
                {
                    TodayAttendance = todayAttendance,
                    PendingEvaluations = pendingEvaluations,
                    TodayEvents = todayEvents,
                    UnreadNotifications = unreadNotifications
                },
                EnrollmentsByGrade = enrollmentsByGrade,
                RecentPerformance = new
                {
                    AverageRecentScore = averageRecentScore,
                    EvaluationsCount = recentGrades.Count
                }
            });
        }
    }
} 