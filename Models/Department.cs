using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models;

public class Department
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [StringLength(80)]
    public string? Building { get; set; }

    public ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
