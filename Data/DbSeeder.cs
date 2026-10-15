using UniversityManagement.Models;

namespace UniversityManagement.Data;

public static class DbSeeder
{
    public static void Seed(UniversityContext db)
    {
        if (db.Departments.Any()) return;

        var cs = new Department { Name = "Computer Science", Building = "Turing Hall" };
        var math = new Department { Name = "Mathematics", Building = "Euler Building" };
        var eng = new Department { Name = "English", Building = "Austen Hall" };
        db.Departments.AddRange(cs, math, eng);

        var i1 = new Instructor { FirstName = "Amira", LastName = "Hassan", Email = "a.hassan@uni.edu", HireDate = new DateTime(2016, 9, 1), Department = cs };
        var i2 = new Instructor { FirstName = "Daniel", LastName = "Okafor", Email = "d.okafor@uni.edu", HireDate = new DateTime(2012, 2, 15), Department = math };
        var i3 = new Instructor { FirstName = "Laila", LastName = "Mansour", Email = "l.mansour@uni.edu", HireDate = new DateTime(2019, 1, 10), Department = eng };
        db.Instructors.AddRange(i1, i2, i3);

        var c1 = new Course { Code = "CS101", Title = "Introduction to Programming", Credits = 4, Department = cs, Instructor = i1 };
        var c2 = new Course { Code = "CS220", Title = "Databases", Credits = 3, Department = cs, Instructor = i1 };
        var c3 = new Course { Code = "MATH150", Title = "Discrete Mathematics", Credits = 3, Department = math, Instructor = i2 };
        var c4 = new Course { Code = "ENG110", Title = "Academic Writing", Credits = 2, Department = eng, Instructor = i3 };
        db.Courses.AddRange(c1, c2, c3, c4);

        var s1 = new Student { FirstName = "Omar", LastName = "Khalil", Email = "omar.khalil@student.uni.edu", DateOfBirth = new DateTime(2005, 4, 12), EnrollmentDate = new DateTime(2023, 9, 1) };
        var s2 = new Student { FirstName = "Sara", LastName = "Nabil", Email = "sara.nabil@student.uni.edu", DateOfBirth = new DateTime(2004, 11, 3), EnrollmentDate = new DateTime(2022, 9, 1) };
        db.Students.AddRange(s1, s2);

        db.Enrollments.AddRange(
            new Enrollment { Student = s1, Course = c1 },
            new Enrollment { Student = s1, Course = c3 },
            new Enrollment { Student = s2, Course = c2 });

        db.SaveChanges();
    }
}
