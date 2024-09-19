using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace proyecto.Controllers
{
    [Authorize]  // Agrega esto a tu controlador o acción
    public class GerenteController : Controller
    {
        private readonly ILogger<GerenteController> _logger;

        private readonly ApplicationDbContext _context;

        public GerenteController(ILogger<GerenteController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IngMat()
        {
            return View();
        }

        public IActionResult ResCosteo()
        {
            return View();
        }

        public IActionResult SalPrend()
        {
            return View();
        }

        public IActionResult VerCostPrev()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCosteo(Costeo model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Guardar el modelo en la base de datos
                    _context.DataCosteo.Add(model);
                    await _context.SaveChangesAsync();

                    // Almacenar un mensaje en TempData para mostrar después de la redirección
                    TempData["SuccessMessage"] = "Costeo registrado exitosamente.";

                    // Redirigir a la página principal
                    return RedirectToAction("Index", "Gerente"); // Cambia "Home" por el controlador deseado si es necesario
                }
                catch (Exception ex)
                {
                    // Registrar el error en la consola
                    _logger.LogError(ex, "Ocurrió un error al registrar el costeo.");

                    // Almacenar un mensaje en TempData para mostrar después de la redirección
                    TempData["ErrorMessage"] = "Ocurrió un error al registrar el costeo: " + ex.Message;

                    // Redirigir a la página principal
                    return RedirectToAction("Index", "Gerente"); // Redirigir incluso en caso de error
                }
            }

            // En caso de error, devolver la vista con el modelo
            return PartialView("_CosteoModal", model);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}