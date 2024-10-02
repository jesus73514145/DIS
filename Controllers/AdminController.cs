using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using proyecto.Models.DTO;
using proyecto.Models.Validator;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Añadir esta línea
using System.Linq;
using Microsoft.EntityFrameworkCore; // Esto es necesario para FirstOrDefaultAsync

/*LIBRERIAS PARA LA PAGINACION DE LISTAR PRODUCTOS */
using X.PagedList;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;

/*validation*/
using FluentValidation;
using FluentValidation.Results;

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



        public async Task<ActionResult> ListUsuGer(int? page)
        {
            var userId = _userManager.GetUserId(User); //sesion

            if (userId == null)
            {
                // no se ha logueado
                TempData["MessageLOGUEARSE"] = "Por favor debe loguearse antes";
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = (page ?? 1); // Si no se especifica la página, asume la página 1
                int pageSize = 10; // maximo 6 usuarios por pagina


                pageNumber = Math.Max(pageNumber, 1);// Con esto se asegura de que pageNumber nunca sea menor que 1
               

                
                // Filtrar usuarios por RolId = 2
                var listaPaginada = await _context.Users
                    .Where(u => u.RolId == "2") // Filtrar por RolId
                    .ToPagedListAsync(pageNumber, pageSize); // Aplicar paginación

                return View("ListUsuGer", listaPaginada);
            }

        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario != null)
            {
                _context.Users.Remove(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListUsuGer));
            }

            return NotFound("no se encontro");
        }

        [HttpGet]
        public async Task<ActionResult> EditarUsuario(string? id)
        {

            ApplicationUser? usuarioEditar = await _context.Users.FindAsync(id);

            if (usuarioEditar == null)
            {
                Console.Write("No se encontro");
                return NotFound("No se encontro a ese usuario");
            }

            UsuarioDTO usuarioEditarDTO = new UsuarioDTO();
            usuarioEditarDTO.Id = usuarioEditar.Id;
            usuarioEditarDTO.Nombres = usuarioEditar.Nombres;
            usuarioEditarDTO.ApellidoPat = usuarioEditar.ApellidoPat;
            usuarioEditarDTO.ApellidoMat = usuarioEditar.ApellidoMat;
            usuarioEditarDTO.NumeroDocumento = usuarioEditar.NumeroDocumento;
            usuarioEditarDTO.TipoDocumento = usuarioEditar.TipoDocumento;
            return View("EditarUsuario", usuarioEditarDTO);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarUsuarioEditado(UsuarioDTO usuarioDTO)
        {

            UsuarioValidator validator = new UsuarioValidator();
            ValidationResult result = validator.Validate(usuarioDTO);

            if (result.IsValid)
            {
                ApplicationUser? user = await _context.Users.FindAsync(usuarioDTO.Id);
                user.Nombres = usuarioDTO.Nombres;
                user.ApellidoPat = usuarioDTO.ApellidoPat;
                user.ApellidoMat = usuarioDTO.ApellidoMat;
                user.TipoDocumento = usuarioDTO.TipoDocumento;
                user.NumeroDocumento = usuarioDTO.NumeroDocumento;

                TempData["MessageActualizandoUsuario"] = "Se Actualizaron exitosamente los datos.";
                _context.Users.Update(user);
                _context.SaveChanges();

                return RedirectToAction("EditarUsuario", new { id = usuarioDTO.Id });
            }

            foreach (var failure in result.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return View("EditarUsuario", usuarioDTO);

        }

        public async Task<IActionResult> buscarUsuario(string query)
        {
            IPagedList<ApplicationUser> usuariosPagedList;

            try
            {

                // Filtrar por RolId = 2
                var baseQuery = _context.Users.Where(u => u.RolId == "2");

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay query, obtener todos los usuarios con RolId = 2
                    var todosLosUsuarios = await baseQuery.ToListAsync();
                    usuariosPagedList = todosLosUsuarios.ToPagedList(1, todosLosUsuarios.Count);
                }
                else
                {
                    // Convertir el query a mayúsculas para una búsqueda sin distinción de mayúsculas
                    query = query.ToUpper();

                    // Filtrar los usuarios por nombre o Id y RolId = 2
                    var usuarios = await baseQuery
                        .Where(p => p.Nombres.ToUpper().Contains(query) || p.Id.ToUpper().Contains(query))
                        .ToListAsync();

                    if (!usuarios.Any())
                    {
                        TempData["MessageDeRespuesta"] = "No se encontraron usuarios que coincidan con la búsqueda.";
                        usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
                    }
                    else
                    {
                        // Asignar a usuariosPagedList la lista paginada de usuarios encontrados
                        usuariosPagedList = usuarios.ToPagedList(1, usuarios.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MessageDeRespuesta"] = "Ocurrió un error al buscar usuarios. Por favor, inténtalo de nuevo más tarde.";
                usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
            }

            // Retorna la vista con usuariosPagedList, que siempre tendrá un valor asignado.
            return View("ListUsuGer", usuariosPagedList);
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
