using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;
using UniversityManagement.Services;

namespace UniversityManagement.Controllers;

public class StudentsController : Controller
{
    private readonly UniversityContext _db;
    private readonly EnrollmentService _enrollments;

    public StudentsController(UniversityContext db, EnrollmentService enrollments)
    {
        _db = db;
        _enrollments = enrollments;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Students.AsNoTracking().Include(s => s.Enrollments).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.FirstName.Contains(term) || s.LastName.Contains(term) || s.Email.Contains(term));
        }
        ViewData["Search"] = search;
        return View(await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var student = await _db.Students
            .Include(s => s.Enrollments).ThenInclude(e => e.Course)
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound();

        var enrolledIds = student.Enrollments.Select(e => e.CourseId).ToList();
        ViewBag.AvailableCourses = new SelectList(
            await _db.Courses.Where(c => !enrolledIds.Contains(c.Id)).OrderBy(c => c.Code)
                .Select(c => new { c.Id, Label = c.Code + " - " + c.Title }).ToListAsync(),
            "Id", "Label");
        return View(student);
    }

    public IActionResult Create() => View(new Student());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Student student)
    {
        await CheckEmailIsUnique(student);
        if (!ModelState.IsValid) return View(student);

        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Added {student.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var student = await _db.Students.FindAsync(id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Student student)
    {
        if (id != student.Id) return NotFound();
        await CheckEmailIsUnique(student);
        if (!ModelState.IsValid) return View(student);

        try
        {
            _db.Update(student);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Students.AnyAsync(s => s.Id == id)) return NotFound();
            throw;
        }
        TempData["Message"] = $"Saved changes to {student.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var student = await _db.Students.Include(s => s.Enrollments).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is not null)
        {
            _db.Students.Remove(student); // enrollments are removed by cascade
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Deleted {student.FullName}.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int studentId, int courseId)
    {
        var (success, message) = await _enrollments.EnrollAsync(studentId, courseId);
        TempData[success ? "Message" : "Error"] = message;
        return RedirectToAction(nameof(Details), new { id = studentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int enrollmentId, int studentId)
    {
        if (await _enrollments.DropAsync(enrollmentId)) TempData["Message"] = "Course dropped.";
        return RedirectToAction(nameof(Details), new { id = studentId });
    }

    private async Task CheckEmailIsUnique(Student s)
    {
        if (await _db.Students.AnyAsync(x => x.Email == s.Email && x.Id != s.Id))
            ModelState.AddModelError(nameof(Student.Email), "Another student already uses this email address.");
    }
}
