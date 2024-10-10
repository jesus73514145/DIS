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
                TempData["MessageLOGUEARSE"] = "Por favor debe loguearse antes";
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = (page ?? 1); // Si no se especifica la página, asume la página 1
                int pageSize = 6; // máximo 6 materiales por página

                pageNumber = Math.Max(pageNumber, 1); // Asegura que pageNumber nunca sea menor que 1

                // Elimina el filtro Where para obtener todos los registros
                var materiales = _context.DataMaterial;

                // Aplicar paginación
                var listaPaginada = await materiales.ToPagedListAsync(pageNumber, pageSize);

                return View("VerMatAdq", listaPaginada);
            }
        }


        /* Para exportar individualmente los productos */
        public async Task<IActionResult> ExportarUnSoloMaterialEnPDF(int id)
        {
            try
            {

                // var product = _context.Producto.Find(id);
                var material = _context.DataMaterial.Find(id);
                if (material == null)
                {
                    return NotFound($"El material con ID {id} no fue encontrado, por eso no se puede exportar en PDF.");
                }

                var html = $@"
            <html>
                <head>
                <meta charset='UTF-8'>
                    <style>
                        table {{
                            width: 100%;
                            border-collapse: collapse;
                        }}
                        th, td {{
                            border: 1px solid black;
                            padding: 8px;
                            text-align: left;
                        }}
                        th {{
                            background-color: #f2f2f2;
                        }}
                        img.logo {{
                            position: absolute;
                            top: 0;
                            right: 0;
                            border-radius:50%;
                            height:3.3rem;
                            width:3.3rem;
                        }}

                        h1 {{
                            color: #40E0D0; /* Color celeste */
                        }}
                    </style>
                </head>
                <body>
                    <img src='https://firebasestorage.googleapis.com/v0/b/proyecto20112023-6e784.appspot.com/o/Fotos_Perfil%2FALZEN_logo.png?alt=media&token=93d06622-a34b-4fdf-96ca-6a8e95839c02' alt='Logo' width='100' class='logo'/>
                    <h1>Reporte de Material {id}</h1>
                    <table>
                        <tr>
                            <th>Modelo</th>
                            <th>Nombre de Tela</th>
                            <th>Precio Total</th>
                            <th>Proveedor</th>
                            <th>Proveedor de Contacto</th>
                            <th>Fecha de Registro</th>
                            <th>Fecha de Actualizacion</th>
                        </tr>";

                html += $@"
                <tr>
                    <td>{material.Modelo}</td>
                    <td>{material.NombreTela}</td>
                    <td>{material.PrecioTotal}</td>
                    <td>{material.Proveedor}</td>
                    <td>{material.ProveedorContacto}</td>
                    <td>{material.FechaRegistro}</td>
                    <td>{material.FechaActualizacion}</td>
                </tr>";


                html += @"
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
                // Loguear el error para obtener más detalles
                _logger.LogError(ex, $"Error al exportar el material {id} a PDF");
                // Retornar un mensaje de error al usuario
                return StatusCode(500, $"Ocurrió un error al exportar el material {id} a PDF. Por favor, inténtelo de nuevo más tarde.");
            }
        }



        public async Task<IActionResult> ExportarUnSoloMaterialEnExcel(int id)
        {
            try
            {

                var material = _context.DataMaterial.Find(id);
                if (material == null)
                {
                    return NotFound($"El material con ID {id} no fue encontrado, por eso no se puede exportar en Excel.");
                }

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Material");

                // Agregando un título arriba de la tabla
                worksheet.Cells[1, 1].Value = $"Reporte del Material {id}";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;

                // Cargar los datos en la fila 3 para dejar espacio para el título
                var materialList = new List<Material> { material };
                worksheet.Cells[3, 1].LoadFromCollection(materialList, true);

                // Dar formato a la tabla Reporte de Productos
                var dataRange = worksheet.Cells[2, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];
                var table = worksheet.Tables.Add(dataRange, "Material");
                table.ShowHeader = true;
                table.TableStyle = TableStyles.Light6;

                // Estilo para los encabezados de las columnas 
                worksheet.Cells[3, 1, 3, worksheet.Dimension.End.Column].Style.Font.Bold = true;
                worksheet.Cells[3, 1, 3, worksheet.Dimension.End.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 1, 3, worksheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 1, 3, worksheet.Dimension.End.Column].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);

                // Ajustar el ancho de las columnas automáticamente
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Material{id}.xlsx");
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
                TempData["MessageLOGUEARSE"] = "Por favor debe loguearse antes de realizar una búsqueda.";
                return View("~/Views/Home/Index.cshtml");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay búsqueda, obtener todos los materiales sin filtrar por usuario
                    var todosLosMateriales = await _context.DataMaterial.ToListAsync();
                    materialPagedList = todosLosMateriales.ToPagedList(1, todosLosMateriales.Count);
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