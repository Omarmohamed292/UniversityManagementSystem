using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;

namespace UniversityManagement.Controllers;

public class CoursesController : Controller
{
    private readonly UniversityContext _db;
    public CoursesController(UniversityContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Courses.AsNoTracking().Include(c => c.Department).Include(c => c.Instructor)
            .Include(c => c.Enrollments).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Code.Contains(term) || c.Title.Contains(term));
        }
        ViewData["Search"] = search;
        return View(await query.OrderBy(c => c.Code).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var course = await _db.Courses.Include(c => c.Department).Include(c => c.Instructor)
            .Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return course is null ? NotFound() : View(course);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new Course());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        await ApplyBusinessRules(course);
        if (!ModelState.IsValid)
        {
            await LoadLists(course);
            return View(course);
        }
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Added {course.Code}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var course = await _db.Courses.FindAsync(id);
        if (course is null) return NotFound();
        await LoadLists(course);
        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.Id) return NotFound();
        await ApplyBusinessRules(course);
        if (!ModelState.IsValid)
        {
            await LoadLists(course);
            return View(course);
        }
        _db.Update(course);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Saved changes to {course.Code}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var course = await _db.Courses.Include(c => c.Department).Include(c => c.Enrollments)
            .AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return course is null ? NotFound() : View(course);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is not null)
        {
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Deleted {course.Code}.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ApplyBusinessRules(Course course)
    {
        course.Code = course.Code?.Trim().ToUpperInvariant() ?? "";
        if (!System.Text.RegularExpressions.Regex.IsMatch(course.Code, @"^[A-Z]{2,4}[0-9]{3}$"))
        {
            ModelState.Remove(nameof(Course.Code));
            ModelState.AddModelError(nameof(Course.Code), "Use 2-4 letters followed by 3 digits, for example CS101.");
        }
        else
        {
            ModelState.Remove(nameof(Course.Code)); // clear any error raised before normalizing the case
        }

        if (await _db.Courses.AnyAsync(c => c.Code == course.Code && c.Id != course.Id))
            ModelState.AddModelError(nameof(Course.Code), "A course with this code already exists.");

        if (course.InstructorId is not null)
        {
            var instructor = await _db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == course.InstructorId);
            if (instructor is null)
                ModelState.AddModelError(nameof(Course.InstructorId), "Choose an instructor from the list.");
            else if (instructor.DepartmentId != course.DepartmentId)
                ModelState.AddModelError(nameof(Course.InstructorId), "The instructor must belong to the course's department.");
        }
    }

    private async Task LoadLists(Course? course = null)
    {
        ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", course?.DepartmentId);
        var instructors = await _db.Instructors.Include(i => i.Department).OrderBy(i => i.LastName)
            .Select(i => new { i.Id, Label = i.FirstName + " " + i.LastName + " (" + i.Department!.Name + ")" }).ToListAsync();
        ViewBag.Instructors = new SelectList(instructors, "Id", "Label", course?.InstructorId);
    }
}
