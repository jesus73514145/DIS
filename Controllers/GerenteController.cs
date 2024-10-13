using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
/*LIBRERIAS PARA LA PAGINACION DE LISTAR USUARIOS GERENTE, SUPERVISOR Y COSTEOS PREVIOS Y MATERIALES ADQUIRIDOS */
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
    [Authorize]   // Agrega esto a tu controlador o acción para que no te redirija a ninguna pagina anterior antes de que se loguee
    public class GerenteController : Controller
    {
        private readonly ILogger<GerenteController> _logger;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Objeto para la exportación
        private readonly IConverter _converter;

        public GerenteController(ILogger<GerenteController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConverter converter)
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

        public async Task<IActionResult> ResCosteo(int id)
        {
            var costeo = await _context.DataCosteo.FindAsync(id);
            if (costeo == null)
            {
                TempData["MessageDeRespuesta"] = "error|Costeo no encontrado.";
                return RedirectToAction("Index", "Gerente");
            }

            // Mostrar datos en la consola
            _logger.LogInformation($"Costeo encontrado: Id={costeo.Id}, Empresa={costeo.Empresa}, CU_Final={costeo.CU_Final}, CT_Final={costeo.CT_Final}, " +
                                   $"Tela1_Costo={costeo.Tela1_Costo}, Tela2_Costo={costeo.Tela2_Costo}, " +
                                   $"CostoTransporte={costeo.CostoTransporte}");

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

        public async Task<ActionResult> SalPrend()
        {
            return View();
        }


        public async Task<ActionResult> VerCostPrev(int? page)
        {
            var userId = _userManager.GetUserId(User); // Obtener el ID del usuario en sesión

            if (userId == null)
            {
                // No se ha logueado
                TempData["MessageDeRespuesta"] = "Por favor, debe iniciar sesión antes de continuar.";
                Console.WriteLine("Usuario no logueado, redirigiendo a la página de inicio."); // Console log
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                int pageNumber = (page ?? 1); // Si no se especifica la página, asume la página 1
                int pageSize = 6; // Máximo 6 costeos por página

                pageNumber = Math.Max(pageNumber, 1); // Asegurarse de que pageNumber nunca sea menor que 1

                try
                {
                    // Elimina el filtro Where para obtener todos los registros
                    var costeos = _context.DataCosteo;

                    // Aplicar paginación
                    var listaPaginada = await costeos.ToPagedListAsync(pageNumber, pageSize);

                    // Mensaje de éxito al cargar los materiales
                    TempData["MessageDeRespuesta"] = "success|Costeos cargados con éxito.";
                    Console.WriteLine("Materiales cargados con éxito."); // Console log
                    
                    return View("VerCostPrev", listaPaginada);
                }
                catch (Exception ex)
                {
                    // En caso de error al obtener los materiales
                    _logger.LogError(ex, "Ocurrió un error al cargar los costeos.");
                    Console.WriteLine($"Error al obtener la lista de costeos: {ex.Message}");
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al intentar cargar los costeos. Por favor, inténtelo más tarde.";
                    return RedirectToAction("Index"); // Redirigir a la vista principal o de error
                }
            }
        }


        /* Para exportar individualmente los productos */
        public async Task<IActionResult> ExportarUnSoloCosteoEnPDF(int id)
        {
            try
            {

                // var product = _context.Producto.Find(id);
                var cost = _context.DataCosteo.Find(id);
                if (cost == null)
                {
                    return NotFound($"El costeo con ID {id} no fue encontrado, por eso no se puede exportar en PDF.");
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
                    <h1>Reporte de Costeo {id}</h1>
                    <table>
                        <tr>
                            <th>ID</th>
                            <th>Empresa</th>
                            <th>Cantidad de Prendas</th>
                            <th>Tela 1</th>
                            <th>Tela 2</th>
                            <th>Costo Final de la Prenda</th>
                            <th>Costo Final del Pedido</th>
                            <th>Fecha de Creación</th>
                        </tr>";

                html += $@"
                <tr>
                    <td>{cost.Id}</td>
                    <td>{cost.Empresa}</td>
                    <td>{cost.Cantidad_Prendas}</td>
                    <td>{cost.Tela1_Nombre}, {cost.Tela1_Costo}</td>
                    <td>{cost.Tela2_Nombre}, {cost.Tela2_Costo}</td>
                    <td>{cost.CU_Final}</td>
                    <td>{cost.CT_Final}</td>
                    <td>{cost.fec_Creacion}</td>
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
                return File(file, "application/pdf", $"Costeo_{id}.pdf");

            }
            catch (Exception ex)
            {
                // Loguear el error para obtener más detalles
                _logger.LogError(ex, $"Error al exportar el costeo {id} a PDF");
                // Retornar un mensaje de error al usuario
                return StatusCode(500, $"Ocurrió un error al exportar el costeo {id} a PDF. Por favor, inténtelo de nuevo más tarde.");
            }
        }



        public async Task<IActionResult> ExportarUnSoloCosteoEnExcel(int id)
        {
            try
            {

                var cost = _context.DataCosteo.Find(id);
                if (cost == null)
                {
                    return NotFound($"El costeo con ID {id} no fue encontrado, por eso no se puede exportar en Excel.");
                }

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Costeo");

                // Agregando un título arriba de la tabla
                worksheet.Cells[1, 1].Value = $"Reporte del Costeo {id}";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;

                // Cargar los datos en la fila 3 para dejar espacio para el título
                var costList = new List<Costeo> { cost };
                worksheet.Cells[3, 1].LoadFromCollection(costList, true);

                // Dar formato a la tabla Reporte de Productos
                var dataRange = worksheet.Cells[2, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];
                var table = worksheet.Tables.Add(dataRange, "Costeo");
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

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Costeo{id}.xlsx");
            }
            catch (Exception ex)
            {
                // Loguear el error para obtener más detalles
                _logger.LogError(ex, $"Error al exportar el costeo {id} a Excel");
                // Retornar un mensaje de error al usuario
                return StatusCode(500, $"Ocurrió un error al exportar el costeo {id} a Excel. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        /* metodo para buscar */

        public async Task<IActionResult> BuscarCosteo(string query)
        {
            // Declara la variable productosPagedList una sola vez aquí
            IPagedList<Costeo> costeosPagedList;

            // Obtén el ID del usuario logueado
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                // Si no hay sesión de usuario activa, muestra un mensaje y redirige
                TempData["MessageDeRespuesta"] = "Por favor debe loguearse antes de realizar una búsqueda.";
                return View("~/Views/Home/Index.cshtml");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay búsqueda, obtener todos los costeos sin filtrar por usuario
                    //var todosLosCosteos = await _context.DataCosteo.ToListAsync();
                    //costeosPagedList = todosLosCosteos.ToPagedList(1, todosLosCosteos.Count);

                    TempData["MessageDeRespuesta"] = "Por favor, ingresa un término de búsqueda.";
                    costeosPagedList = new PagedList<Costeo>(new List<Costeo>(), 1, 1); // Lista vacía
                }
                else
                {
                    // Si hay una búsqueda, aplica el filtro
                    query = query.ToUpper();
                    var costeos = await _context.DataCosteo
                        .Where(p => p.Empresa.ToUpper().Contains(query)) // Solo filtra por la búsqueda, no por usuario
                        .ToListAsync();

                    if (!costeos.Any())
                    {
                        TempData["MessageDeRespuesta"] = "error|No se encontraron costeos que coincidan con la búsqueda.";
                        costeosPagedList = new PagedList<Costeo>(new List<Costeo>(), 1, 1);
                    }
                    else
                    {
                        TempData["MessageDeRespuesta"] = "success|Se encontraron costeos que coinciden con la búsqueda."; // Mensaje de éxito
                        costeosPagedList = costeos.ToPagedList(1, costeos.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones
                Console.WriteLine($"Error al buscar costeos: {ex.Message}");
                TempData["MessageDeRespuesta"] = "error|Ocurrió un error al buscar costeos. Por favor, inténtalo de nuevo más tarde.";
                costeosPagedList = new PagedList<Costeo>(new List<Costeo>(), 1, 1);
            }

            // Retorna la vista con costeosPagedList, que siempre tendrá un valor asignado.
            return View("VerCostPrev", costeosPagedList);
        }




        public async Task<ActionResult> Costeo()
        {
            return View();
        }

        public async Task<ActionResult> DetalleCosteo(int id)
        {
            // Intenta encontrar el costeo por ID
            Costeo costeo = await _context.DataCosteo.FindAsync(id);

            if (costeo == null)
            {
                // Si no se encuentra el costeo, maneja el error
                TempData["MessageDeRespuesta"] = "error|No se encontró el costeo solicitado.";
                Console.WriteLine($"Costeo con ID {id} no encontrado."); // Registra el error en la consola
                return NotFound("No se encontró el costeo."); // Respuesta HTTP 404
            }

            // Retorna la vista con el costeo encontrado
            return View(costeo);
        }


        [HttpPost]
        public async Task<IActionResult> RegistrarCosteo(Costeo model)
        {
            // Convertir valores nulos a 0 antes de realizar cualquier cálculo
            model.Cantidad_Prendas = model.Cantidad_Prendas ?? 0;

            model.Tela1_Costo = model.Tela1_Costo ?? 0;
            model.Tela1_Cantidad = model.Tela1_Cantidad ?? 0;
            model.Precio_Tela1 = model.Precio_Tela1 ?? 0;

            model.Tela2_Costo = model.Tela2_Costo ?? 0;
            model.Tela2_Cantidad = model.Tela2_Cantidad ?? 0;
            model.Precio_Tela2 = model.Precio_Tela2 ?? 0;

            model.Molde = model.Molde ?? 0;
            model.Tizado = model.Tizado ?? 0;
            model.Corte = model.Corte ?? 0;
            model.Confección = model.Confección ?? 0;
            model.Botones = model.Botones ?? 0;
            model.Pegado_Botón = model.Pegado_Botón ?? 0;
            model.Otros = model.Otros ?? 0;
            model.Avios = model.Avios ?? 0;
            model.Tricotex = model.Tricotex ?? 0;
            model.Acabados = model.Acabados ?? 0;
            model.Precio_Transporte = model.Precio_Transporte ?? 0;


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

            model.UserID = _userManager.GetUserId(User);
            model.fec_Creacion = DateTime.Now.ToUniversalTime();
            model.fec_Actualizacion = null;

            // Revisar si el ModelState es válido después de las validaciones personalizadas
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Guardando el modelo en la base de datos.");
                    _context.DataCosteo.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["MessageDeRespuesta"] = "success|Costeo registrado con éxito.";


                    // Redirigir a la vista de detalles con el ID del costeo recién creado
                    return RedirectToAction("ResCosteo", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al registrar el costeo.");
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al registrar el costeo: " + ex.Message;

                    Console.WriteLine($"Error al registrar el costeo: {ex.Message}"); // Log en consola
                    return RedirectToAction("Index", "Gerente");
                }
            }

            // Obtener todos los errores del ModelState y convertirlos en una cadena.
            var errorMessages = ModelState.Values
                                          .SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            // para mapear que error de que campo es
            //TempData["MessageDeRespuesta"] = string.Join(" ", errorMessages);

            // Si algo falla, devolver el modelo con los errores y los datos de vuelta a la vista
            return View("Costeo", model);
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
                    return RedirectToAction("Index", "Gerente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al registrar el material.");
                    TempData["MessageDeRespuesta"] = "error|Ocurrió un error al registrar el material: " + ex.Message;
                    return RedirectToAction("Index", "Gerente");
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
                TempData["MessageDeRespuesta"] = "Por favor debe loguearse antes de realizar una búsqueda.";
                return View("~/Views/Home/Index.cshtml");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // Si no hay búsqueda, obtener todos los materiales sin filtrar por usuario
                    //var todosLosMateriales = await _context.DataMaterial.ToListAsync();
                    //materialPagedList = todosLosMateriales.ToPagedList(1, todosLosMateriales.Count);

                    // Si el query está vacío o solo contiene espacios, muestra un mensaje al usuario
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