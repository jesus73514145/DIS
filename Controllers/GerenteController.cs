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

        public GerenteController(ILogger<GerenteController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IngMat()
        {
            return View();
        }

        public IActionResult CostPrend()
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}