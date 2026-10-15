using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;

namespace UniversityManagement.Controllers;

public class InstructorsController : Controller
{
    private readonly UniversityContext _db;
    public InstructorsController(UniversityContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Instructors.AsNoTracking().Include(i => i.Department).Include(i => i.Courses).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i => i.FirstName.Contains(term) || i.LastName.Contains(term) || i.Email.Contains(term));
        }
        ViewData["Search"] = search;
        return View(await query.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var instructor = await _db.Instructors.Include(i => i.Department)
            .Include(i => i.Courses).AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return instructor is null ? NotFound() : View(instructor);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDepartments();
        return View(new Instructor());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Instructor instructor)
    {
        await CheckEmailIsUnique(instructor);
        if (!ModelState.IsValid)
        {
            await LoadDepartments(instructor.DepartmentId);
            return View(instructor);
        }
        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Added {instructor.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor is null) return NotFound();
        await LoadDepartments(instructor.DepartmentId);
        return View(instructor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Instructor instructor)
    {
        if (id != instructor.Id) return NotFound();
        await CheckEmailIsUnique(instructor);

        // Business rule: an instructor cannot move to another department while teaching its courses.
        var current = await _db.Instructors.AsNoTracking().Include(i => i.Courses).FirstOrDefaultAsync(i => i.Id == id);
        if (current is null) return NotFound();
        if (current.DepartmentId != instructor.DepartmentId && current.Courses.Any())
            ModelState.AddModelError(nameof(Instructor.DepartmentId),
                "Reassign this instructor's courses before changing their department.");

        if (!ModelState.IsValid)
        {
            await LoadDepartments(instructor.DepartmentId);
            return View(instructor);
        }
        _db.Update(instructor);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Saved changes to {instructor.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var instructor = await _db.Instructors.Include(i => i.Department).Include(i => i.Courses)
            .AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return instructor is null ? NotFound() : View(instructor);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor is not null)
        {
            _db.Instructors.Remove(instructor); // courses keep existing with no instructor (SET NULL)
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Deleted {instructor.FullName}.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartments(int? selected = null) =>
        ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", selected);

    private async Task CheckEmailIsUnique(Instructor i)
    {
        if (await _db.Instructors.AnyAsync(x => x.Email == i.Email && x.Id != i.Id))
            ModelState.AddModelError(nameof(Instructor.Email), "Another instructor already uses this email address.");
    }
}
