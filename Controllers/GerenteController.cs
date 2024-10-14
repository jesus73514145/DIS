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


        /* Para exportar individualmente los costeos */
        public async Task<IActionResult> ExportarUnSoloCosteoEnPDF(int id)
        {
            try
            {
                var cost = await _context.DataCosteo.FindAsync(id);
                if (cost == null)
                {
                    return NotFound($"El costeo con ID {id} no fue encontrado, por eso no se puede exportar en PDF.");
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
                        color: #000000; 
                        text-align: center;
                    }}
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
                        border-radius: 50%;
                        height: 3.3rem;
                        width: 3.3rem;
                    }}
                    
                    .section {{
                        margin-bottom: 20px;
                    }}
                </style>
            </head>
            <body>
                <img src='https://firebasestorage.googleapis.com/v0/b/proyecto20112023-6e784.appspot.com/o/Fotos_Perfil%2FALZEN_logo.png?alt=media&token=93d06622-a34b-4fdf-96ca-6a8e95839c02' alt='Logo' width='100' class='logo'/>
                <h1>Reporte de Costeo {id}</h1>
                
                <div class='section'>
                    <h2>Información General</h2>
                    <table>
                        <tr>
                            <th>Código del Costeo</th>
                            <td>{cost.Id}</td>
                        </tr>
                        <tr>
                            <th>Empresa</th>
                            <td>{cost.Empresa}</td>
                        </tr>
                        <tr>
                            <th>Cantidad de Prendas</th>
                            <td>{cost.Cantidad_Prendas}</td>
                        </tr>
                        <tr>
                            <th>Fecha de Creación</th>
                            <td>{cost.fec_Creacion}</td>
                        </tr>
                        <tr>
                            <th>Fecha de Actualización</th>
                            <td>{cost.fec_Actualizacion?.ToString() ?? "N/A"}</td>
                        </tr>
                    </table>
                </div>

                <div class='section'>
                    <h2>Detalles de Costos</h2>
                    <table>
                        <tr>
                            <th>Tela 1</th>
                            <td>{cost.Tela1_Nombre} - Costo: S/. {cost.Tela1_Costo}, Cantidad: {cost.Tela1_Cantidad}</td>
                        </tr>
                        <tr>
                            <th>Tela 2</th>
                            <td>{cost.Tela2_Nombre} - Costo: S/. {cost.Tela2_Costo}, Cantidad: {cost.Tela2_Cantidad ?? 0}</td>
                        </tr>
                        <tr>
                            <th>Costo del Molde</th>
                            <td>S/. {cost.Molde}</td>
                        </tr>
                        <tr>
                            <th>Costo del Tizado</th>
                            <td>S/. {cost.Tizado}</td>
                        </tr>
                        <tr>
                            <th>Costo del Corte</th>
                            <td>{cost.Corte}</td>
                        </tr>
                        <tr>
                            <th>Costo de Confección</th>
                            <td>S/. {cost.Confección}</td>
                        </tr>
                        <tr>
                            <th>Costo de Botones</th>
                            <td>S/. {cost.Botones}</td>
                        </tr>
                        <tr>
                            <th>Costo de Pegado de Botón</th>
                            <td>S/. {cost.Pegado_Botón}</td>
                        </tr>
                        <tr>
                            <th>Costo de Otros</th>
                            <td>S/. {cost.Otros ?? 0}</td>
                        </tr>
                        <tr>
                            <th>Costo de Avíos</th>
                            <td>S/. {cost.Avios}</td>
                        </tr>
                        <tr>
                            <th>Costo de Tricotex</th>
                            <td>S/. {cost.Tricotex}</td>
                        </tr>
                        <tr>
                            <th>Costo de Acabados</th>
                            <td>S/. {cost.Acabados}</td>
                        </tr>
                        <tr>
                            <th>Costo de Transporte</th>
                            <td>S/. {cost.CostoTransporte}</td>
                        </tr>
                        <tr>
                            <th>Costo Final de la Prenda</th>
                            <td>S/. {Math.Round(cost.CU_Final ?? 0, 2).ToString("F2")}</td>
                        </tr>
                        <tr>
                            <th>Costo Final del Pedido</th>
                            <td>S/. {Math.Round(cost.CT_Final ?? 0, 2).ToString("F2")}</td>
                        </tr>
                    </table>
                </div>
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
                var cost = await _context.DataCosteo.FindAsync(id);
                if (cost == null)
                {
                    return NotFound($"El costeo con ID {id} no fue encontrado, por eso no se puede exportar en Excel.");
                }

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add($"Costeo_{id}");

                // Título
                worksheet.Cells[1, 1].Value = $"Reporte del Costeo {id}";
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
                worksheet.Cells[3, 1].Value = "Detalles del Costeo:";
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;

                // Llenar datos en filas
                int filaDatos = 5; // Número de fila donde comienzas a llenar los datos
                worksheet.Cells[filaDatos, 1].Value = "Código del Costeo:";
                worksheet.Cells[filaDatos, 2].Value = cost.Id;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Empresa:";
                worksheet.Cells[filaDatos, 2].Value = cost.Empresa;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Cantidad de Prendas:";
                worksheet.Cells[filaDatos, 2].Value = cost.Cantidad_Prendas;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Fecha de Creación:";
                worksheet.Cells[filaDatos, 2].Value = cost.fec_Creacion;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Fecha de Actualización:";
                worksheet.Cells[filaDatos, 2].Value = cost.fec_Actualizacion?.ToString() ?? "N/A";

                // Sección de telas
                filaDatos += 2; // Espacio entre secciones
                worksheet.Cells[filaDatos, 1].Value = "Detalles de Telas:";
                worksheet.Cells[filaDatos, 1].Style.Font.Bold = true;
                worksheet.Cells[filaDatos, 1].Style.Font.Size = 16;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Tela 1:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela1_Nombre;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo Tela 1:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela1_Costo;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Cantidad Tela 1:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela1_Cantidad;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Tela 2:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela2_Nombre;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo Tela 2:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela2_Costo;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Cantidad Tela 2:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tela2_Cantidad ?? 0;

                // Sección de costos
                filaDatos += 2; // Espacio entre secciones
                worksheet.Cells[filaDatos, 1].Value = "Costos Asociados:";
                worksheet.Cells[filaDatos, 1].Style.Font.Bold = true;
                worksheet.Cells[filaDatos, 1].Style.Font.Size = 16;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo del Molde:";
                worksheet.Cells[filaDatos, 2].Value = cost.Molde;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo del Tizado:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tizado;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo del Corte:";
                worksheet.Cells[filaDatos, 2].Value = cost.Corte;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Confección:";
                worksheet.Cells[filaDatos, 2].Value = cost.Confección;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Botones:";
                worksheet.Cells[filaDatos, 2].Value = cost.Botones;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Pegado de Botón:";
                worksheet.Cells[filaDatos, 2].Value = cost.Pegado_Botón;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Otros:";
                worksheet.Cells[filaDatos, 2].Value = cost.Otros;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Avíos:";
                worksheet.Cells[filaDatos, 2].Value = cost.Avios;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Tricotex:";
                worksheet.Cells[filaDatos, 2].Value = cost.Tricotex;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Acabados:";
                worksheet.Cells[filaDatos, 2].Value = cost.Acabados;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo de Transporte:";
                worksheet.Cells[filaDatos, 2].Value = cost.CostoTransporte;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo Final de la Prenda:";
                worksheet.Cells[filaDatos, 2].Value = cost.CU_Final;

                filaDatos++;
                worksheet.Cells[filaDatos, 1].Value = "Costo Final del Pedido:";
                worksheet.Cells[filaDatos, 2].Value = cost.CT_Final;

                // Estilo de las celdas
                for (int i = 5; i <= 9; i++)
                {
                    worksheet.Cells[i, 1].Style.Font.Bold = true;
                    worksheet.Cells[i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Estilo de las celdas
                for (int i = 12; i <= 17; i++)
                {
                    worksheet.Cells[i, 1].Style.Font.Bold = true;
                    worksheet.Cells[i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Estilo de las celdas
                for (int i = 20; i <= 32; i++)
                {
                    worksheet.Cells[i, 1].Style.Font.Bold = true;
                    worksheet.Cells[i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Ajustar ancho de las columnas
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Costeo_{id}.xlsx");
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