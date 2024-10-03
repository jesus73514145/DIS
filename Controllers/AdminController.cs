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
            // Busca el usuario en la base de datos
            ApplicationUser? usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                Console.WriteLine("No se encontró");
                return NotFound("No se encontró a ese usuario");
            }

            // Imprimir las propiedades del usuario en la consola
            Console.WriteLine("Usuario encontrado:");
            Console.WriteLine($"ID: {usuario.Id}");
            Console.WriteLine($"Nombres: {usuario.Nombres}");
            Console.WriteLine($"Apellido Paterno: {usuario.ApellidoPat}");
            Console.WriteLine($"Apellido Materno: {usuario.ApellidoMat}");
            Console.WriteLine($"Tipo de Documento: {usuario.TipoDocumento}");
            Console.WriteLine($"Número de Documento: {usuario.NumeroDocumento}");
            Console.WriteLine($"Género: {usuario.Genero}");
            Console.WriteLine($"Email: {usuario.Email}");
            Console.WriteLine($"Celular: {usuario.Celular}");
            Console.WriteLine($"PasswordHash: {usuario.PasswordHash}");

            // Crea un nuevo objeto UserRegistrationViewModel y mapea las propiedades
            UserRegistrationViewModel usuarioEditar = new UserRegistrationViewModel
            {
                Id = usuario.Id, // Asegúrate de que el Id esté en el ViewModel
                Nombres = usuario.Nombres,
                ApellidoPat = usuario.ApellidoPat,
                ApellidoMat = usuario.ApellidoMat,
                TipoDocumento = usuario.TipoDocumento,
                NumeroDocumento = usuario.NumeroDocumento,
                Genero = usuario.Genero,
                Email = usuario.Email,
                Celular = usuario.Celular,
                Password = usuario.PasswordHash, // Nota: Considera no exponer la contraseña
                ConfirmPassword = usuario.PasswordHash // Nota: Considera no exponer la contraseña
                                                       // Agrega otras propiedades según sea necesario
            };

            return View("EditarUsuario", usuarioEditar); // Pasa el ViewModel a la vista
        }



        [HttpPost]
        public async Task<IActionResult> GuardarUsuarioEditado(UserRegistrationViewModel usuarioDTO)
        {
            _logger.LogInformation("Iniciando la edición del usuario con ID: {UserId}", usuarioDTO.Id);

            // Verificar si el modelo es válido
            if (!ModelState.IsValid)
            {
                // Registrar los errores del modelo
                _logger.LogWarning("Modelo no es válido.");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Error en el campo {Field}: {ErrorMessage}", error.Key, error.Value.Errors.First().ErrorMessage);
                    }
                }
                return View("EditarUsuario", usuarioDTO); // Regresar a la vista con el modelo
            }

            // Buscar el usuario en la base de datos
            ApplicationUser? user = await _context.Users.FindAsync(usuarioDTO.Id);

            if (user == null)
            {
                _logger.LogWarning("El usuario con ID {UserId} no fue encontrado.", usuarioDTO.Id);
                ModelState.AddModelError(string.Empty, "El usuario no fue encontrado.");
                return View("EditarUsuario", usuarioDTO); // Regresar a la vista con el modelo
            }

            // Validar unicidad del número de documento
            var existingUserByDoc = await _context.Users
                .FirstOrDefaultAsync(u => u.NumeroDocumento == usuarioDTO.NumeroDocumento && u.Id != usuarioDTO.Id);

            if (existingUserByDoc != null)
            {
                ModelState.AddModelError(string.Empty, "El número de documento ya está en uso.");
                _logger.LogWarning("Número de documento ya está en uso.");
                return View("EditarUsuario", usuarioDTO);
            }

            // Validar unicidad del correo electrónico
            var existingUserByEmail = await _userManager.FindByEmailAsync(usuarioDTO.Email);

            if (existingUserByEmail != null && existingUserByEmail.Id != usuarioDTO.Id)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");
                return View("EditarUsuario", usuarioDTO);
            }

            // Actualizar los datos del usuario
            user.Nombres = usuarioDTO.Nombres;
            user.ApellidoPat = usuarioDTO.ApellidoPat;
            user.ApellidoMat = usuarioDTO.ApellidoMat;
            user.TipoDocumento = usuarioDTO.TipoDocumento;
            user.NumeroDocumento = usuarioDTO.NumeroDocumento;
            user.Email = usuarioDTO.Email; // Asegúrate de actualizar el email
            user.Celular = usuarioDTO.Celular;
            user.Genero = usuarioDTO.Genero;
            user.fechaDeActualizacion = DateTime.Now.ToUniversalTime();
            user.Activo = true;

            // Verificar si se debe actualizar la contraseña
            if (!string.IsNullOrEmpty(usuarioDTO.Password) || !string.IsNullOrEmpty(usuarioDTO.ConfirmPassword))
            {
                if (usuarioDTO.Password != usuarioDTO.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                    _logger.LogWarning("Las contraseñas no coinciden.");
                    return View("EditarUsuario", usuarioDTO);
                }

                // Validar la complejidad de la contraseña (por ejemplo, longitud mínima)
                if (usuarioDTO.Password.Length < 6) // Cambia esto según tus requisitos de complejidad
                {
                    ModelState.AddModelError(string.Empty, "La contraseña debe tener al menos 6 caracteres.");
                    _logger.LogWarning("La contraseña no cumple con los requisitos de complejidad.");
                    return View("EditarUsuario", usuarioDTO);
                }

                // Establecer la nueva contraseña
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, usuarioDTO.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditarUsuario", usuarioDTO);
                }
            }

            try
            {
                // Actualizar el usuario en el contexto
                _context.Users.Update(user);
                await _context.SaveChangesAsync(); // Guardar cambios de manera asíncrona

                _logger.LogInformation("Usuario con ID {UserId} editado exitosamente.", usuarioDTO.Id);
                TempData["MessageActualizandoUsuario"] = "Se actualizaron exitosamente los datos.";
                return RedirectToAction("EditarUsuario", new { id = usuarioDTO.Id });
            }
            catch (Exception ex)
            {
                // Manejar errores de base de datos
                _logger.LogError("Error al guardar los cambios para el usuario con ID {UserId}: {ErrorMessage}", usuarioDTO.Id, ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios: " + ex.Message);
            }

            // Si hay errores de validación, vuelve a cargar la vista con el modelo
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
