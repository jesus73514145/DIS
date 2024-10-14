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
using Microsoft.AspNetCore.Mvc.Rendering; // Añadir esta línea para renderizar
using System.Linq;
using Microsoft.EntityFrameworkCore; // Esto es necesario para FirstOrDefaultAsync

/*LIBRERIAS PARA LA PAGINACION DE LISTAR DE USUARIOS GERENTE, LISTAR DE USUARIOS SUPERVISOR */
using X.PagedList;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;

/*validation*/
using FluentValidation;
using FluentValidation.Results;

namespace proyecto.Controllers
{
    [Authorize]  // Agrega esto a tu controlador o acción para que no te redirija a ninguna pagina anterior antes de que se loguee
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
            var userId = _userManager.GetUserId(User); // Obtener el ID de usuario de la sesión actual

            if (userId == null)
            {
                // No se ha iniciado sesión, redirigir a la página de inicio y mostrar un mensaje
                TempData["MessageDeRespuesta"] = "Por favor, debe iniciar sesión antes de continuar.";
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = page ?? 1; // Si no se especifica la página, asume la página 1
                int pageSize = 10; // Número máximo de usuarios por página

                // Asegurarse de que pageNumber nunca sea menor que 1
                pageNumber = Math.Max(pageNumber, 1);

                try
                {
                    // Filtrar usuarios por RolId = 2 y aplicar paginación
                    var listaPaginada = await _context.Users
                        .Where(u => u.RolId == "2") // Filtrar por RolId
                        .ToPagedListAsync(pageNumber, pageSize); // Aplicar paginación

                    // Si la lista está vacía, mostrar un mensaje de respuesta
                    if (!listaPaginada.Any())
                    {
                        TempData["MessageDeRespuesta"] = "No se encontraron usuarios con el rol especificado.";
                    }

                    return View("ListUsuGer", listaPaginada);
                }
                catch (Exception ex)
                {
                    // En caso de error, mostrar un mensaje de respuesta con el mensaje de error
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al cargar los usuarios. Intente nuevamente más tarde.";
                    return View("ListUsuGer", new List<ApplicationUser>().ToPagedList(1, pageSize));
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> EliminarUsuarioGerente(string id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario != null)
            {
                _context.Users.Remove(usuario);
                await _context.SaveChangesAsync();

                TempData["MessageDeRespuesta"] = "success|El usuario ha sido eliminado exitosamente.";
                return RedirectToAction(nameof(ListUsuGer));
            }

            TempData["MessageDeRespuesta"] = "error|No se encontró el usuario especificado.";
            return RedirectToAction(nameof(ListUsuGer));
        }


        [HttpPost]
        public async Task<IActionResult> EliminarUsuarioSupervisor(string id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario != null)
            {
                _context.Users.Remove(usuario);
                await _context.SaveChangesAsync();

                TempData["MessageDeRespuesta"] = "success|El usuario ha sido eliminado exitosamente.";
                return RedirectToAction(nameof(ListUsuSup));
            }

            TempData["MessageDeRespuesta"] = "error|No se encontró el usuario especificado.";
            return RedirectToAction(nameof(ListUsuSup));
        }


        [HttpGet]
        public async Task<ActionResult> EditarUsuario(string? id)
        {
            // Busca el usuario en la base de datos
            ApplicationUser? usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                Console.WriteLine("No se encontró el usuario especificado.");
                TempData["MessageDeRespuesta"] = "error|No se encontró el usuario especificado.";
                return RedirectToAction("ListUsuGer"); // Redirige a la lista de usuarios
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
            UserEditViewModel usuarioEditar = new UserEditViewModel
            {
                Id = usuario.Id, // Asegúrate de que el Id esté en el ViewModel
                Nombres = usuario.Nombres,
                ApellidoPat = usuario.ApellidoPat,
                ApellidoMat = usuario.ApellidoMat,
                TipoDocumento = usuario.TipoDocumento,
                NumeroDocumento = usuario.NumeroDocumento,
                Genero = usuario.Genero,
                Email = usuario.Email,
                Celular = usuario.Celular
                //Password = usuario.PasswordHash, // Nota: Considera no exponer la contraseña
                //ConfirmPassword = usuario.PasswordHash // Nota: Considera no exponer la contraseña
                                                       // Agrega otras propiedades según sea necesario
            };

            return View("EditarUsuario", usuarioEditar); // Pasa el ViewModel a la vista
        }




      /*  [HttpPost]
        public async Task<IActionResult> GuardarUsuarioEditado(UserEditViewModel usuarioDTO)
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
                TempData["MessageDeRespuesta"] = "error|Hay errores en el formulario. Por favor, corrígelos."; // Mensaje de error
                return View("EditarUsuario", usuarioDTO); // Regresar a la vista con el modelo
            }
            // Buscar el usuario en la base de datos
            ApplicationUser? user = await _context.Users.FindAsync(usuarioDTO.Id);

            if (user == null)
            {
                _logger.LogWarning("El usuario con ID {UserId} no fue encontrado.", usuarioDTO.Id);
                TempData["MessageDeRespuesta"] = "error|El usuario no fue encontrado."; // Mensaje de error
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
                TempData["MessageDeRespuesta"] = "El número de documento ya está en uso."; // Mensaje de error
                return View("EditarUsuario", usuarioDTO);
            }

            // Validar unicidad del correo electrónico
            var existingUserByEmail = await _userManager.FindByEmailAsync(usuarioDTO.Email);

            if (existingUserByEmail != null && existingUserByEmail.Id != usuarioDTO.Id)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");
                TempData["MessageDeRespuesta"] = "El correo electrónico ya está en uso."; // Mensaje de error
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
                    TempData["MessageDeRespuesta"] = "error|Las contraseñas no coinciden."; // Mensaje de error
                    return View("EditarUsuario", usuarioDTO);
                }

                // Validar la complejidad de la contraseña (por ejemplo, longitud mínima)
                if (usuarioDTO.Password.Length < 6) // Cambia esto según tus requisitos de complejidad
                {
                    ModelState.AddModelError(string.Empty, "La contraseña debe tener al menos 6 caracteres.");
                    _logger.LogWarning("La contraseña no cumple con los requisitos de complejidad.");
                    TempData["MessageDeRespuesta"] = "error|La contraseña debe tener al menos 6 caracteres."; // Mensaje de error
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
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al restablecer la contraseña."; // Mensaje de error
                    return View("EditarUsuario", usuarioDTO);
                }
            }

            try
            {
                // Actualizar el usuario en el contexto
                _context.Users.Update(user);
                await _context.SaveChangesAsync(); // Guardar cambios de manera asíncrona

                _logger.LogInformation("Usuario con ID {UserId} editado exitosamente.", usuarioDTO.Id);
                TempData["MessageActualizandoUsuario"] = "success|Se actualizaron exitosamente los datos."; // Mensaje de éxito
                return RedirectToAction("EditarUsuario", new { id = usuarioDTO.Id });
            }
            catch (Exception ex)
            {
                // Manejar errores de base de datos
                _logger.LogError("Error al guardar los cambios para el usuario con ID {UserId}: {ErrorMessage}", usuarioDTO.Id, ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios: " + ex.Message);
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al guardar los cambios: " + ex.Message; // Mensaje de error
            }

            // Si hay errores de validación, vuelve a cargar la vista con el modelo
            return View("EditarUsuario", usuarioDTO);
        }*/

         [HttpPost]
        public async Task<IActionResult> GuardarUsuarioEditado(UserEditViewModel usuarioDTO)
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
                TempData["MessageDeRespuesta"] = "error|Hay errores en el formulario. Por favor, corrígelos."; // Mensaje de error
                return View("EditarUsuario", usuarioDTO); // Regresar a la vista con el modelo
            }
            // Buscar el usuario en la base de datos
            ApplicationUser? user = await _context.Users.FindAsync(usuarioDTO.Id);

            if (user == null)
            {
                _logger.LogWarning("El usuario con ID {UserId} no fue encontrado.", usuarioDTO.Id);
                TempData["MessageDeRespuesta"] = "error|El usuario no fue encontrado."; // Mensaje de error
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
                TempData["MessageDeRespuesta"] = "El número de documento ya está en uso."; // Mensaje de error
                return View("EditarUsuario", usuarioDTO);
            }

            // Validar unicidad del correo electrónico
            var existingUserByEmail = await _userManager.FindByEmailAsync(usuarioDTO.Email);

            if (existingUserByEmail != null && existingUserByEmail.Id != usuarioDTO.Id)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");
                TempData["MessageDeRespuesta"] = "El correo electrónico ya está en uso."; // Mensaje de error
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

           

            try
            {
                // Actualizar el usuario en el contexto
                _context.Users.Update(user);
                await _context.SaveChangesAsync(); // Guardar cambios de manera asíncrona

                _logger.LogInformation("Usuario con ID {UserId} editado exitosamente.", usuarioDTO.Id);
                TempData["MessageDeRespuesta"] = "success|Se actualizaron exitosamente los datos."; // Mensaje de éxito
                return RedirectToAction("EditarUsuario", new { id = usuarioDTO.Id });
            }
            catch (Exception ex)
            {
                // Manejar errores de base de datos
                _logger.LogError("Error al guardar los cambios para el usuario con ID {UserId}: {ErrorMessage}", usuarioDTO.Id, ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios: " + ex.Message);
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al guardar los cambios: " + ex.Message; // Mensaje de error
            }

            // Si hay errores de validación, vuelve a cargar la vista con el modelo
            return View("EditarUsuario", usuarioDTO);
        }




        public async Task<IActionResult> buscarUsuarioGer(string query)
        {
            IPagedList<ApplicationUser> usuariosPagedList;

            try
            {

                // Filtrar por RolId = 2
                var baseQuery = _context.Users.Where(u => u.RolId == "2");

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay query, obtener todos los usuarios con RolId = 2
                    //var todosLosUsuarios = await baseQuery.ToListAsync();
                    //usuariosPagedList = todosLosUsuarios.ToPagedList(1, todosLosUsuarios.Count);

                    // Si el query está vacío o solo contiene espacios, muestra un mensaje al usuario
                    TempData["MessageDeRespuesta"] = "Por favor, ingresa un término de búsqueda.";
                    usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1); // Lista vacía
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
                        TempData["MessageDeRespuesta"] = "error|No se encontraron usuarios que coincidan con la búsqueda.";
                        usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
                    }
                    else
                    {
                        // Asignar a usuariosPagedList la lista paginada de usuarios encontrados
                        TempData["MessageDeRespuesta"] = "success|Se encontraron usuarios que coinciden con la búsqueda."; // Mensaje de éxito
                        usuariosPagedList = usuarios.ToPagedList(1, usuarios.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al buscar usuarios. Por favor, inténtalo de nuevo más tarde.";
                usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
            }

            // Retorna la vista con usuariosPagedList, que siempre tendrá un valor asignado.
            return View("ListUsuGer", usuariosPagedList);
        }

        public async Task<IActionResult> buscarUsuarioSup(string query)
        {
            IPagedList<ApplicationUser> usuariosPagedList;

            try
            {

                // Filtrar por RolId = 2
                var baseQuery = _context.Users.Where(u => u.RolId == "3");

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay query, obtener todos los usuarios con RolId = 2
                    //var todosLosUsuarios = await baseQuery.ToListAsync();
                    //usuariosPagedList = todosLosUsuarios.ToPagedList(1, todosLosUsuarios.Count);

                    // Si el query está vacío o solo contiene espacios, muestra un mensaje al usuario
                    TempData["MessageDeRespuesta"] = "Por favor, ingresa un término de búsqueda.";
                    usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1); // Lista vacía
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
                        TempData["MessageDeRespuesta"] = "error|No se encontraron usuarios que coincidan con la búsqueda.";
                        usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
                    }
                    else
                    {
                        // Asignar a usuariosPagedList la lista paginada de usuarios encontrados
                        TempData["MessageDeRespuesta"] = "success|Se encontraron usuarios que coinciden con la búsqueda."; // Mensaje de éxito
                        usuariosPagedList = usuarios.ToPagedList(1, usuarios.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al buscar usuarios. Por favor, inténtalo de nuevo más tarde.";
                usuariosPagedList = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 1);
            }

            // Retorna la vista con usuariosPagedList, que siempre tendrá un valor asignado.
            return View("ListUsuSup", usuariosPagedList);
        }


        [HttpGet]
        public async Task<ActionResult> ListUsuSup(int? page)
        {
            var userId = _userManager.GetUserId(User); // sesión

            if (userId == null)
            {
                // no se ha logueado
                TempData["MessageDeRespuesta"] = "success|Por favor debe loguearse antes"; // Mensaje unificado
                Console.WriteLine("Intento de acceso sin iniciar sesión."); // Consola
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = (page ?? 1); // Si no se especifica la página, asume la página 1
                int pageSize = 10; // máximo 10 usuarios por página

                pageNumber = Math.Max(pageNumber, 1); // Asegura que pageNumber nunca sea menor que 1

                // Filtrar usuarios por RolId = 3
                var listaPaginada = await _context.Users
                    .Where(u => u.RolId == "3") // Filtrar por RolId
                    .ToPagedListAsync(pageNumber, pageSize); // Aplicar paginación

                if (!listaPaginada.Any()) // Validar si la lista está vacía
                {
                    TempData["MessageDeRespuesta"] = "error|No se encontraron usuarios en esta categoría."; // Mensaje cuando la lista está vacía
                    Console.WriteLine("No se encontraron usuarios para mostrar."); // Consola
                }
                else
                {
                    Console.WriteLine($"Lista de usuarios para la página {pageNumber} cargada exitosamente."); // Consola
                }

                return View("ListUsuSup", listaPaginada);
            }
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

                TempData["MessageDeRespuesta"] = "error|Por favor, corrija los errores en el formulario."; // Mensaje unificado
                return View();
            }

            // Verificar si el número de documento ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.NumeroDocumento == model.NumeroDocumento);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "El número de documento ya está en uso.");
                _logger.LogWarning("Número de documento ya está en uso.");

                TempData["MessageDeRespuesta"] = "error|El número de documento ya está en uso."; // Mensaje unificado
                return View();
            }

            // Verificar si el correo electrónico ya existe
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingUserByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");

                TempData["MessageDeRespuesta"] = "error|El correo electrónico ya está en uso."; // Mensaje unificado
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
                    TempData["MessageDeRespuesta"] = "success|Usuario rol gerente agregado con éxito."; // Mensaje unificado
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
            TempData["MessageDeRespuesta"] = "error|Ocurrió un error al registrar el usuario."; // Mensaje unificado
            return View();
        }



        [HttpGet]
        public IActionResult AgregUsuSup()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregUsuSup(UserRegistrationViewModel model)
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

                TempData["MessageDeRespuesta"] = "error|Por favor, corrija los errores en el formulario."; // Mensaje unificado
                return View();
            }

            // Verificar si el número de documento ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.NumeroDocumento == model.NumeroDocumento);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "El número de documento ya está en uso.");
                _logger.LogWarning("Número de documento ya está en uso.");

                TempData["MessageDeRespuesta"] = "error|El número de documento ya está en uso."; // Mensaje unificado
                return View();
            }

            // Verificar si el correo electrónico ya existe
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingUserByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                _logger.LogWarning("Correo electrónico ya está en uso.");

                TempData["MessageDeRespuesta"] = "error|El correo electrónico ya está en uso."; // Mensaje unificado
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
            user.RolId = "3";  // Almacenar el RolId en el ApplicationUser
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
                    TempData["MessageDeRespuesta"] = "success|Usuario rol supervisor agregado con éxito."; // Mensaje unificado
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
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al registrar el usuario."; // Mensaje unificado
            }

            // Si hay errores de validación, vuelve a cargar la vista con el modelo
            return View();
        }

        public async Task<IActionResult> VolverAtras(string? id)
        {
            // Busca el usuario por su ID
            ApplicationUser? usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                // Manejo si no se encuentra el usuario
                TempData["MessageDeRespuesta"] = "error|No se encontró el usuario solicitado.";
                return RedirectToAction("Index"); // Redirige a la página principal o a otra acción
            }

            // Redirigir según el RolId
            if (usuario.RolId == "2")
            {
                return RedirectToAction("ListusuGer"); // Redirige a la lista de usuarios Gerente
            }
            else if (usuario.RolId == "3")
            {
                return RedirectToAction("ListusuSup"); // Redirige a la lista de usuarios Supervisor
            }

            // Si no se cumple ninguna condición, podrías manejar otro caso o redirigir a un error
            TempData["MessageDeRespuesta"] = "error|Rol de usuario no reconocido.";
            return RedirectToAction("Index"); // O cualquier otra acción por defecto
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


    }
}
