using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;

namespace UniversityManagement.Controllers;

public class HomeController : Controller
{
    private readonly UniversityContext _db;
    public HomeController(UniversityContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Students = await _db.Students.CountAsync();
        ViewBag.Instructors = await _db.Instructors.CountAsync();
        ViewBag.Courses = await _db.Courses.CountAsync();
        ViewBag.Unstaffed = await _db.Courses.CountAsync(c => c.InstructorId == null);
        return View();
    }

    public IActionResult Error() => View();
}
