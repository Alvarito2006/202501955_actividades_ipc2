using Microsoft.AspNetCore.Mvc;

namespace ControlAcademicoMvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Listar", "Estudiante");
    }
}
