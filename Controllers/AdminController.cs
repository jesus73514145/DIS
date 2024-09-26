using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Añadir esta línea
using System.Linq;
using Microsoft.EntityFrameworkCore; // Esto es necesario para FirstOrDefaultAsync

namespace proyecto.Controllers
{
    [Authorize]  // Agrega esto a tu controlador o acción
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMyEmailSender _emailSender;

        public AdminController(ILogger<AdminController> logger,
            ApplicationDbContext context, IMyEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult ListUsuGer()
        {
            return View("ListUsuGer");
        }

        [HttpGet]
        public IActionResult ListUsuSup()
        {
            return View("ListUsuSup");
        }

        [HttpGet]
        public IActionResult AgreUsu()
        {

            return View();
        }

        [HttpGet]
        public IActionResult AgregUsuGer()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregUsuGer(UserRegistrationViewModel model)
        {
            _logger.LogInformation("Iniciando el registro de usuario.");

            if (!ModelState.IsValid)
            {
                // Registrar los errores del modelo
                _logger.LogWarning("Modelo no es válido.");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Error en el campo {0}: {1}", error.Key, error.Value.Errors.First().ErrorMessage);
                    }
                }


                return View();
            }

            // Verificar si el número de documento ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.NumeroDocumento == model.NumeroDocumento);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "El número de documento ya está en uso.");
                _logger.LogWarning("Número de documento ya está en uso.");


                return View();
            }

            // Verificar si el correo electrónico ya existe
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingUserByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");


                return View();
            }

            // Crear el nuevo usuario
            var user = Activator.CreateInstance<ApplicationUser>();
            user.UserName = model.Email;  // Se debe establecer el UserName para Identity
            user.Email = model.Email;
            user.Nombres = model.Nombres;
            user.ApellidoPat = model.ApellidoPat;
            user.ApellidoMat = model.ApellidoMat;
            user.TipoDocumento = model.TipoDocumento;
            user.NumeroDocumento = model.NumeroDocumento;
            user.Celular = model.Celular;
            user.Genero = model.Genero;
            user.RolId = "2";  // Almacenar el RolId en el ApplicationUser
            user.fechaDeRegistro = DateTime.Now.ToUniversalTime();
            user.fechaDeActualizacion = null;
            user.Activo = true;

            // Crear el usuario en la base de datos
            var result = await _userManager.CreateAsync(user, model.Password);


            if (result.Succeeded)
            {
                // Generar el token de confirmación de correo
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Confirmar el email automáticamente utilizando el token generado
                var confirmResult = await _userManager.ConfirmEmailAsync(user, token);

                if (confirmResult.Succeeded)
                {
                    _logger.LogInformation("Usuario registrado con éxito y correo confirmado automáticamente.");
                    TempData["SuccessMessage"] = "Usuario rol gerente agregado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError("Error al confirmar el correo automáticamente.");
                    foreach (var error in confirmResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                // Manejar errores de creación de usuario
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }


            // Si hay errores de validación, vuelve a cargar la vista con el modelo
            return View();
        }

        [HttpGet]
        public IActionResult AgregUsuSup()
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
