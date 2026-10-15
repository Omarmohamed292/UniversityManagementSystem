using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a course code."),
     RegularExpression(@"^[A-Z]{2,4}[0-9]{3}$", ErrorMessage = "Use 2-4 capital letters followed by 3 digits, for example CS101.")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Enter a course title."), StringLength(100)]
    public string Title { get; set; } = "";

    [Range(1, 6, ErrorMessage = "Credits must be between 1 and 6.")]
    public int Credits { get; set; } = 3;

    [Display(Name = "Department"), Range(1, int.MaxValue, ErrorMessage = "Choose a department.")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Display(Name = "Instructor")]
    public int? InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
