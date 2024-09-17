// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using proyecto.Models;
using proyecto.Data;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace proyecto.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMyEmailSender _emailSender;

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IMyEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public SelectList TipoDocumentos { get; set; }
        public SelectList Roles { get; set; }  // Cambiar de List<string> a SelectList
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>


            [Required(ErrorMessage = "El campo de correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
            [Display(Name = "Correo Electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El campo Nombres es obligatorio.")]
            [Display(Name = "Nombres")]
            public string Nombres { get; set; }

            [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio.")]
            [Display(Name = "Apellido Paterno")]
            public string ApellidoPat { get; set; }

            [Required(ErrorMessage = "El campo Apellido Materno es obligatorio.")]
            [Display(Name = "Apellido Materno")]
            public string ApellidoMat { get; set; }


            [Required]
            [Display(Name = "Tipo de Documento")]
            public string TipoDocumento { get; set; }

            [Required(ErrorMessage = "El Numero de Documento es obligatorio.")]
            [Display(Name = "Numero de Documento")]
            public string NumeroDocumento { get; set; }



            [Required(ErrorMessage = "Por favor, selecciona un género.")]
            [Display(Name = "Género")]
            public string Genero { get; set; }

            [Required(ErrorMessage = "El número de celular es obligatorio.")]
            [RegularExpression(@"^(\d{9})$", ErrorMessage = "El número de celular debe tener 9 dígitos.")]
            [Display(Name = "Celular")]
            public string Celular { get; set; }

            // Campo para Estado
            [Required]
            public bool Activo { get; set; }

            // Campo para Rol
            [Required(ErrorMessage = "El rol es obligatorio.")]
            [Display(Name = "Rol")]
            public string RolId { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Llama al método para cargar los datos necesarios
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Verificar si el número de documento ya existe en la base de datos
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.NumeroDocumento == Input.NumeroDocumento);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "El número de documento ya está en uso.");
                    await LoadDataAsync();
                    return Page();
                }

                // Verificar si el correo electrónico ya existe en la base de datos
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);

                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "El correo electrónico ya está en uso.");
                    await LoadDataAsync();
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("El usuario creó una nueva cuenta con contraseña.");



                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirme su email",
                        $"Por favor confirme su cuenta por <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si falló la validación, vuelve a cargar las listas desplegables
            await LoadDataAsync();

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                var user = Activator.CreateInstance<ApplicationUser>();
                user.Nombres = Input.Nombres;
                user.ApellidoPat = Input.ApellidoPat;
                user.ApellidoMat = Input.ApellidoMat;
                user.TipoDocumento = Input.TipoDocumento;
                user.NumeroDocumento = Input.NumeroDocumento;
                user.Celular = Input.Celular;
                user.Genero = Input.Genero;
                user.RolId = Input.RolId;
                user.fechaDeRegistro = DateTime.Now.ToUniversalTime();
                user.fechaDeActualizacion = null;
                user.Activo = true; // esta línea es para asignar
                return user;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            var emailStore = _userStore as IUserEmailStore<ApplicationUser>;
            if (emailStore == null)
            {
                throw new InvalidCastException("The user store does not implement IUserEmailStore<ApplicationUser>.");
            }
            return emailStore;
        }

        private async Task LoadDataAsync()
        {
            // Cargar roles
            var roles = await _roleManager.Roles.ToListAsync();
            Roles = new SelectList(roles, "Id", "Name");

            // Cargar tipos de documentos
            var tiposDocumentos = new List<SelectListItem>
    {
        new SelectListItem { Value = "DNI", Text = "DNI" },
        new SelectListItem { Value = "Carnet", Text = "Carnet" },
        new SelectListItem { Value = "Pasaporte", Text = "Pasaporte" },
        // Agrega aquí más tipos de documentos según sea necesario
    };
            TipoDocumentos = new SelectList(tiposDocumentos, "Value", "Text");
        }

    }
}