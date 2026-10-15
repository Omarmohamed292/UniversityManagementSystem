using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models;

public class Instructor : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a first name."), StringLength(50), Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Enter a last name."), StringLength(50), Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Enter an email address."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(100)]
    public string Email { get; set; } = "";

    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Hire date")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [Display(Name = "Department"), Range(1, int.MaxValue, ErrorMessage = "Choose a department.")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public string FullName => $"{FirstName} {LastName}";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HireDate > DateTime.Today)
            yield return new ValidationResult("The hire date cannot be in the future.", new[] { nameof(HireDate) });
    }
}
