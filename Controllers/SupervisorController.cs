using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
/*LIBRERIAS PARA LA PAGINACION DE LISTAR LOS MATERIALES ADQUIRIDOS */
using X.PagedList;

/*LIBRERIAS NECESARIAS PARA EXPORTAR EN PDF */
using DinkToPdf;
using DinkToPdf.Contracts;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace proyecto.Controllers
{
    [Authorize]  // Agrega esto a tu controlador o acción para que no te redirija a ninguna pagina anterior antes de que se loguee
    public class SupervisorController : Controller
    {
        private readonly ILogger<SupervisorController> _logger;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Objeto para la exportación
        private readonly IConverter _converter;

        public SupervisorController(ILogger<SupervisorController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConverter converter)
        {
            _logger = logger;
            _context = context;

            _userManager = userManager;

            _converter = converter; // PARA EXPORTAR
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> IngMat()
        {
            return View();
        }





        [HttpPost]
        public async Task<IActionResult> RegistrarMaterial(Material model)
        {
            // Convertir valores nulos a 0 antes de realizar cualquier cálculo
            model.Cantidad = model.Cantidad ?? 0;
            model.Precio = model.Precio ?? 0;
            model.PrecioTotal = model.PrecioTotal ?? 0;

            model.UserID = _userManager.GetUserId(User);
            model.FechaRegistro = DateTime.Now.ToUniversalTime();
            model.FechaActualizacion = null;


            // Revisar si el ModelState es válido después de las validaciones personalizadas
            if (ModelState.IsValid)
            {

                try
                {
                    _logger.LogInformation("Guardando el modelo en la base de datos.");
                    _context.DataMaterial.Add(model);
                    await _context.SaveChangesAsync();

                    // Establecer un mensaje de éxito en TempData
                    string successMessage = "success|Material registrado exitosamente.";
                    TempData["MessageDeRespuesta"] = successMessage;

                    // Registrar el mensaje en consola
                    _logger.LogInformation(successMessage);

                    // Redirigir a la vista "Index" del controlador "Gerente"
                    return RedirectToAction("Index", "Supervisor");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al registrar el material.");
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al registrar el material: " + ex.Message;
                    return RedirectToAction("Index", "Supervisor");
                }
            }

            // Obtener todos los errores del ModelState y convertirlos en una cadena.
            var errorMessages = ModelState.Values
                                          .SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            //TempData["ErrorMessage"] = string.Join(" ", errorMessages);
            // Si algo falla, devolver el modelo con los errores y los datos de vuelta a la vista
            return View("IngMat", model);
        }





        public async Task<ActionResult> VerMatAdq(int? page)
        {
            var userId = _userManager.GetUserId(User); // sesión

            if (userId == null)
            {
                // no se ha logueado
                TempData["MessageDeRespuesta"] = "Por favor, debe iniciar sesión antes de continuar.";
                Console.WriteLine("Usuario no logueado, redirigiendo a la página de inicio."); // Console log
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = (page ?? 1); // Si no se especifica la página, asume la página 1
                int pageSize = 6; // máximo 6 materiales por página

                pageNumber = Math.Max(pageNumber, 1); // Asegura que pageNumber nunca sea menor que 1

                try
                {
                    // Elimina el filtro Where para obtener todos los registros
                    var materiales = _context.DataMaterial;

                    // Aplicar paginación
                    var listaPaginada = await materiales.ToPagedListAsync(pageNumber, pageSize);

                    // Mensaje de éxito al cargar los materiales
                    TempData["MessageDeRespuesta"] = "success|Materiales cargados con éxito.";
                    Console.WriteLine("Materiales cargados con éxito."); // Console log

                    return View("VerMatAdq", listaPaginada);
                }
                catch (Exception ex)
                {
                    // En caso de error al obtener los materiales
                    _logger.LogError(ex, "Ocurrió un error al cargar los materiales.");
                    Console.WriteLine("Error al cargar los materiales: " + ex.Message); // Console log
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al cargar los materiales: " + ex.Message;
                    return View("VerMatAdq", null); // o redirigir a otra vista si lo prefieres
                }
            }
        }


        /* Para exportar individualmente los materiales */
        public async Task<IActionResult> ExportarUnSoloMaterialEnPDF(int id)
        {
            try
            {
                var material = await _context.DataMaterial.FindAsync(id);
                if (material == null)
                {
                    return NotFound($"El material con ID {id} no fue encontrado, por eso no se puede exportar en PDF.");
                }

                // Obtener información del usuario que agregó el material desde la base de datos.
                var usuario = await _context.Users.FindAsync(material.UserID);
                if (usuario == null)
                {
                    return NotFound($"El usuario con ID {material.UserID} no fue encontrado.");
                }

                var html = $@"
        <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        margin: 20px;
                        background-color: #f8f8f8;
                    }}
                    h1 {{
                        color: #000000; /* Color celeste */
                        text-align: center;
                    }}
                    h2 {{
                        color: #333;
                        text-align: left;
                        margin-top: 40px;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin: 20px 0;
                    }}
                    th, td {{
                        border: 1px solid #ddd;
                        padding: 12px;
                        text-align: left;
                    }}
                    th {{
                        background-color: #000000; /* Color celeste */
                        color: white;
                        font-weight: bold;
                    }}
                    tr:nth-child(even) {{
                        background-color: #f9f9f9; /* Color de fila alternada */
                    }}
                    tr:hover {{
                        background-color: #e0f7fa; /* Color de fila al pasar el ratón */
                    }}
                    img.logo {{
                        position: absolute;
                        top: 10px;
                        right: 10px;
                        border-radius: 50%;
                        height: 50px;
                        width: 50px;
                    }}
                </style>
            </head>
            <body>
                <img src='https://firebasestorage.googleapis.com/v0/b/proyecto20112023-6e784.appspot.com/o/Fotos_Perfil%2FALZEN_logo.png?alt=media&token=93d06622-a34b-4fdf-96ca-6a8e95839c02' alt='Logo' class='logo'/>
                <h1>Reporte de Material {id}</h1>
                <h2>Información del Material</h2>
                <table>
                    <tr>
                        <th>ID</th>
                        <th>Modelo</th>
                        <th>Nombre de Tela</th>
                        <th>Precio por Metro</th>
                        <th>Cantidad (metros)</th>
                        <th>Precio Total</th>
                        <th>Proveedor</th>
                        <th>Proveedor de Contacto</th>
                        <th>Fecha de Registro</th>
                        <th>Fecha de Actualización</th>
                    </tr>
                    <tr>
                        <td>{material.Id}</td>
                        <td>{material.Modelo}</td>
                        <td>{material.NombreTela}</td>
                        <td>{material.Precio:C2}</td>
                        <td>{material.Cantidad}</td>
                        <td>{material.PrecioTotal:C2}</td>
                        <td>{material.Proveedor}</td>
                        <td>{material.ProveedorContacto}</td>
                        <td>{material.FechaRegistro:dd/MM/yyyy}</td>
                        <td>{material.FechaActualizacion?.ToString("dd/MM/yyyy") ?? "N/A"}</td>
                    </tr>
                </table>

                <h2>Información del Usuario que Agregó el Material</h2>
                <table>
                    <tr>
                        <th>Nombre Completo</th>
                        <th>Email</th>
                        <th>Numero de Documento</th>
                        <th>Celular</th>
                        <th>Fecha de Registro</th>  
                    </tr>
                    <tr>
                        <td>{usuario.Nombres} {usuario.ApellidoPat} {usuario.ApellidoMat}</td>
                        <td>{usuario.Email}</td>
                        <td>{usuario.NumeroDocumento}</td>
                        <td>{usuario.Celular}</td>
                        <td>{usuario.fechaDeRegistro:dd/MM/yyyy}</td>
                    </tr>
                </table>
            </body>
        </html>";

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                };
                var objectSettings = new ObjectSettings
                {
                    HtmlContent = html
                };
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };
                var file = _converter.Convert(pdf);
                return File(file, "application/pdf", $"Material_{id}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al exportar el material {id} a PDF");
                return StatusCode(500, $"Ocurrió un error al exportar el material {id} a PDF. Por favor, inténtelo de nuevo más tarde.");
            }
        }




        public async Task<IActionResult> ExportarUnSoloMaterialEnExcel(int id)
        {
            try
            {
                var material = await _context.DataMaterial.FindAsync(id);
                if (material == null)
                {
                    return NotFound($"El material con ID {id} no fue encontrado, por eso no se puede exportar en Excel.");
                }

                // Obtener información del usuario que agregó el material desde la base de datos.
                var usuario = await _context.Users.FindAsync(material.UserID);
                if (usuario == null)
                {
                    return NotFound($"El usuario con ID {material.UserID} no fue encontrado.");
                }

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add($"Material_{id}");

                // Título
                worksheet.Cells[1, 1].Value = $"Reporte del Material {id}";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1, 1, 3].Merge = true; // Fusionar celdas para el título

                // Descargar la imagen del logo
                using var client = new HttpClient();
                var logoBytes = await client.GetByteArrayAsync("https://firebasestorage.googleapis.com/v0/b/proyecto20112023-6e784.appspot.com/o/Fotos_Perfil%2FALZEN_logo.png?alt=media&token=93d06622-a34b-4fdf-96ca-6a8e95839c02");

                // Agregar la imagen al archivo Excel
                var image = worksheet.Drawings.AddPicture("Logo", new MemoryStream(logoBytes));
                image.SetPosition(0, 15, 3, 0); // Coloca el logo en la fila 1, columna E
                image.SetSize(100, 100);  // Establece el tamaño de la imagen


                // Espacio para el título
                worksheet.Cells[3, 1].Value = "Detalles del Material:";
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;

                // Llenar datos en filas
                int filaDatos = 5; // Número de fila donde comienzas a llenar los datos
                worksheet.Cells[filaDatos, 1].Value = "Código del Material:";
                worksheet.Cells[filaDatos, 2].Value = material.Id;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Modelo:";
                worksheet.Cells[filaDatos, 2].Value = material.Modelo;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Nombre de Tela:";
                worksheet.Cells[filaDatos, 2].Value = material.NombreTela;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Precio por Metro:";
                worksheet.Cells[filaDatos, 2].Value = material.Precio.HasValue ? material.Precio.Value.ToString("C2") : "N/A";

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Cantidad (metros):";
                worksheet.Cells[filaDatos, 2].Value = material.Cantidad.ToString();

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Precio Total:";
                worksheet.Cells[filaDatos, 2].Value = material.PrecioTotal.HasValue ? material.PrecioTotal.Value.ToString("C2") : "N/A";

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Proveedor:";
                worksheet.Cells[filaDatos, 2].Value = material.Proveedor;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Proveedor de Contacto:";
                worksheet.Cells[filaDatos, 2].Value = material.ProveedorContacto;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Fecha de Registro:";
                worksheet.Cells[filaDatos, 2].Value = material.FechaRegistro != null ? ((DateTime)material.FechaRegistro).ToString("dd/MM/yyyy") : "N/A";

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Fecha de Actualización:";
                worksheet.Cells[filaDatos, 2].Value = material.FechaActualizacion.HasValue ? material.FechaActualizacion.Value.ToString("dd/MM/yyyy") : "N/A";

                // Sección de telas
                filaDatos += 2; // Espacio entre secciones
                worksheet.Cells[filaDatos, 1].Value = "Información del Usuario:";
                worksheet.Cells[filaDatos, 1].Style.Font.Bold = true;
                worksheet.Cells[filaDatos, 1].Style.Font.Size = 16;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Nombre Completo Usuario:";
                worksheet.Cells[filaDatos, 2].Value = $"{usuario.Nombres} {usuario.ApellidoPat} {usuario.ApellidoMat}";

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Email Usuario:";
                worksheet.Cells[filaDatos, 2].Value = usuario.Email;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Numero de Documento Usuario:";
                worksheet.Cells[filaDatos, 2].Value = usuario.NumeroDocumento;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Celular Usuario:";
                worksheet.Cells[filaDatos, 2].Value = usuario.Celular;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Fecha de Registro Usuario:";
                worksheet.Cells[filaDatos, 2].Value = usuario.fechaDeRegistro.HasValue ? usuario.fechaDeRegistro.Value.ToString("dd/MM/yyyy") : "N/A";



                // Estilo de las celdas
                for (int i = 5; i <= 14; i++)
                {
                    worksheet.Cells[i, 1].Style.Font.Bold = true;
                    worksheet.Cells[i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Estilo de las celdas
                for (int i = 17; i <= 21; i++)
                {
                    worksheet.Cells[i, 1].Style.Font.Bold = true;
                    worksheet.Cells[i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }



                // Ajustar ancho de las columnas
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Material_{id}.xlsx");
            }
            catch (Exception ex)
            {
                // Loguear el error para obtener más detalles
                _logger.LogError(ex, $"Error al exportar el material {id} a Excel");
                // Retornar un mensaje de error al usuario
                return StatusCode(500, $"Ocurrió un error al exportar el material {id} a Excel. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        /* metodo para buscar */

        public async Task<IActionResult> BuscarMaterial(string query)
        {
            // Declara la variable materialPagedList una sola vez aquí
            IPagedList<Material> materialPagedList;

            // Obtén el ID del usuario logueado
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                // Si no hay sesión de usuario activa, muestra un mensaje y redirige
                TempData["MessageDeRespuesta"] = "Por favor, debe iniciar sesión antes de continuar.";
                return View("~/Views/Home/Index.cshtml");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay búsqueda, obtener todos los materiales sin filtrar por usuario
                    //var todosLosMateriales = await _context.DataMaterial.ToListAsync();
                    //materialPagedList = todosLosMateriales.ToPagedList(1, todosLosMateriales.Count);

                    TempData["MessageDeRespuesta"] = "Por favor, ingresa un término de búsqueda.";
                    materialPagedList = new PagedList<Material>(new List<Material>(), 1, 1); // Lista vacía
                }
                else
                {
                    // Si hay una búsqueda, aplica el filtro solo en el campo Modelo
                    query = query.ToUpper();
                    var materiales = await _context.DataMaterial
                        .Where(p => p.Modelo.ToUpper().Contains(query)) // Filtra solo por la búsqueda
                        .ToListAsync();

                    if (!materiales.Any())
                    {
                        TempData["MessageDeRespuesta"] = "error|No se encontraron materiales que coincidan con la búsqueda.";
                        materialPagedList = new PagedList<Material>(new List<Material>(), 1, 1);
                    }
                    else
                    {
                        TempData["MessageDeRespuesta"] = "success|Se encontraron materiales que coinciden con la búsqueda."; // Mensaje de éxito
                        materialPagedList = materiales.ToPagedList(1, materiales.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al buscar el material. Por favor, inténtalo de nuevo más tarde.";
                materialPagedList = new PagedList<Material>(new List<Material>(), 1, 1);
            }

            // Retorna la vista con materialPagedList, que siempre tendrá un valor asignado.
            return View("VerMatAdq", materialPagedList);
        }


        [HttpPost]
        public async Task<IActionResult> EliminarMaterial(int id)
        {
            var material = await _context.DataMaterial.FindAsync(id);

            if (material != null)
            {
                _context.DataMaterial.Remove(material);
                await _context.SaveChangesAsync();
                TempData["MessageDeRespuesta"] = "success|El material ha sido eliminado correctamente.";
                return RedirectToAction(nameof(VerMatAdq));
            }
            else
            {
                TempData["MessageDeRespuesta"] = "error|No se pudo eliminar el material, no fue encontrado.";
                return RedirectToAction(nameof(VerMatAdq));
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}