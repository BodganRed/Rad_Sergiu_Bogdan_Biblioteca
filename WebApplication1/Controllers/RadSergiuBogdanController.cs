using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RadSergiuBogdanController : Controller
    {
        public IActionResult Index()
        {
            var student = new Student { Name = "Ana", Age = 21 };
            return View(student);

        }
    }
}
