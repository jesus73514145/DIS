using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using proyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace proyecto.Controllers;
[Authorize]  // Agrega esto a tu controlador o acción para que no te redirija a ninguna pagina anterior antes de que se loguee
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMyEmailSender _emailSender;
    public HomeController(ILogger<HomeController> logger,
        ApplicationDbContext context, IMyEmailSender emailSender,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;

        /* lineas agregadas */
        _context = context;

        _emailSender = emailSender;

        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }


    public async Task<IActionResult> Index()
    {
        return View();
    }












    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
