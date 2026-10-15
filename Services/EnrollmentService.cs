using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;

namespace UniversityManagement.Services;

/// <summary>Business rules for enrolling students in courses.</summary>
public class EnrollmentService
{
    public const int MaxCoursesPerStudent = 6;
    private readonly UniversityContext _db;

    public EnrollmentService(UniversityContext db) => _db = db;

    public async Task<(bool Success, string Message)> EnrollAsync(int studentId, int courseId)
    {
        var student = await _db.Students.FindAsync(studentId);
        var course = await _db.Courses.FindAsync(courseId);
        if (student is null || course is null)
            return (false, "The student or course no longer exists.");

        if (await _db.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId))
            return (false, $"{student.FullName} is already enrolled in {course.Code}.");

        var count = await _db.Enrollments.CountAsync(e => e.StudentId == studentId);
        if (count >= MaxCoursesPerStudent)
            return (false, $"A student can take at most {MaxCoursesPerStudent} courses. Drop one before adding another.");

        _db.Enrollments.Add(new Enrollment { StudentId = studentId, CourseId = courseId });
        await _db.SaveChangesAsync();
        return (true, $"{student.FullName} is now enrolled in {course.Code}.");
    }

    public async Task<bool> DropAsync(int enrollmentId)
    {
        var enrollment = await _db.Enrollments.FindAsync(enrollmentId);
        if (enrollment is null) return false;
        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync();
        return true;
    }
}
