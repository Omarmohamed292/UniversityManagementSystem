using Microsoft.EntityFrameworkCore;
using UniversityManagement.Models;

namespace UniversityManagement.Data;

public class UniversityContext : DbContext
{
    public UniversityContext(DbContextOptions<UniversityContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Student>().HasIndex(s => s.Email).IsUnique();
        b.Entity<Instructor>().HasIndex(i => i.Email).IsUnique();
        b.Entity<Course>().HasIndex(c => c.Code).IsUnique();
        b.Entity<Department>().HasIndex(d => d.Name).IsUnique();

        // A department cannot be removed while it still owns instructors or courses.
        b.Entity<Instructor>().HasOne(i => i.Department).WithMany(d => d.Instructors)
            .HasForeignKey(i => i.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Course>().HasOne(c => c.Department).WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        // Deleting an instructor leaves their courses unassigned.
        b.Entity<Course>().HasOne(c => c.Instructor).WithMany(i => i.Courses)
            .HasForeignKey(c => c.InstructorId).OnDelete(DeleteBehavior.SetNull);

        // A student can enroll in a course only once.
        b.Entity<Enrollment>().HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        b.Entity<Enrollment>().HasOne(e => e.Student).WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Enrollment>().HasOne(e => e.Course).WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
