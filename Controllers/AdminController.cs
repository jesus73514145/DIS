using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.Extensions.Logging;
using proyecto.Data;
using proyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

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

            /* lineas agregadas */
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
            return View("AgreUsu");
        }

        [HttpGet]
        public IActionResult UsuGer()
        {
            return View("UsuGer");
        }

        [HttpGet]
        public IActionResult UsuSup()
        {
            return View("UsuSup");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}