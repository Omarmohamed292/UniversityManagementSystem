using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models;

public class Student : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a first name."), StringLength(50), Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Enter a last name."), StringLength(50), Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Enter an email address."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(100)]
    public string Email { get; set; } = "";

    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Date of birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Enrollment date")]
    public DateTime EnrollmentDate { get; set; } = DateTime.Today;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public string FullName => $"{FirstName} {LastName}";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth > DateTime.Today.AddYears(-16))
            yield return new ValidationResult("Students must be at least 16 years old.", new[] { nameof(DateOfBirth) });
        if (EnrollmentDate > DateTime.Today)
            yield return new ValidationResult("The enrollment date cannot be in the future.", new[] { nameof(EnrollmentDate) });
    }
}
