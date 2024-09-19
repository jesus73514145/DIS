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

        public async Task<IActionResult> ResCosteo(int id)
        {
            var costeo = await _context.DataCosteo.FindAsync(id);
            if (costeo == null)
            {
                TempData["ErrorMessage"] = "Costeo no encontrado.";
                return RedirectToAction("Index", "Gerente");
            }

            // Calcular la suma de los costos (excepto CostoTransporte)
            var sumaCostos = costeo.Molde + costeo.Tizado + costeo.Corte + costeo.Confección +
                             costeo.Botones + costeo.Pegado_Botón + costeo.Otros +
                             costeo.Avios + costeo.Tricotex + costeo.Acabados;

            var vistaModelo = new CosteoDetallesViewModel
            {
                Id = costeo.Id,
                CU_Final = costeo.CU_Final,
                Empresa = costeo.Empresa,
                CT_Final = costeo.CT_Final,
                Tela1_Costo = costeo.Tela1_Costo,
                Tela2_Costo = costeo.Tela2_Costo,
                SumaCostos = sumaCostos,
                CostoTransporte = costeo.CostoTransporte
            };

            return View(vistaModelo);
        }

        public IActionResult SalPrend()
        {
            return View();
        }

        public IActionResult VerCostPrev()
        {
            return View();
        }

        public IActionResult Costeo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCosteo(Costeo model)
        {
            // Calcular CU_Final y CT_Final antes de cualquier validación
            model.CU_Final = model.Tela1_Costo + model.Tela2_Costo + model.Molde + model.Tizado +
                             model.Corte + model.Confección + model.Botones + model.Pegado_Botón +
                             model.Otros + model.Avios + model.Tricotex + model.Acabados + model.CostoTransporte;

            model.CT_Final = model.CU_Final * model.Cantidad_Prendas;

            // Verificar que los cálculos se hayan realizado correctamente
            if (model.CU_Final <= 0)
            {
                ModelState.AddModelError("CU_Final", "El costo unitario debe ser mayor a 0.");
            }

            if (model.CT_Final <= 0)
            {
                ModelState.AddModelError("CT_Final", "El costo total debe ser mayor a 0.");
            }

            // Revisar si el ModelState es válido después de las validaciones personalizadas
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Guardando el modelo en la base de datos.");
                    _context.DataCosteo.Add(model);
                    await _context.SaveChangesAsync();

                    // Redirigir a la vista de detalles con el ID del costeo recién creado
                    return RedirectToAction("ResCosteo", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al registrar el costeo.");
                    TempData["ErrorMessage"] = "Ocurrió un error al registrar el costeo: " + ex.Message;
                    return RedirectToAction("Index", "Gerente");
                }
            }

            // Obtener todos los errores del ModelState y convertirlos en una cadena.
            var errorMessages = ModelState.Values
                                          .SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();

            TempData["ErrorMessage"] = string.Join(" ", errorMessages);

            // Si algo falla, devolver el modelo con los errores y los datos de vuelta a la vista
            return View("Costeo", model);
        }







        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}