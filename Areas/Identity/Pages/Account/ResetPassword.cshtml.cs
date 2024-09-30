// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using proyecto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace proyecto.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El campo de correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
            public string Email { get; set; }

            /// <summary>
            ///     Este API soporta la infraestructura predeterminada de la interfaz de usuario de ASP.NET Core Identity y no está destinado
            ///     a ser utilizado directamente desde su código. Este API puede cambiar o ser removido en versiones futuras.
            /// </summary>
            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(20, ErrorMessage = "La Contraseña debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     Este API soporta la infraestructura predeterminada de la interfaz de usuario de ASP.NET Core Identity y no está destinado
            ///     a ser utilizado directamente desde su código. Este API puede cambiar o ser removido en versiones futuras.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            ///     Este API soporta la infraestructura predeterminada de la interfaz de usuario de ASP.NET Core Identity y no está destinado
            ///     a ser utilizado directamente desde su código. Este API puede cambiar o ser removido en versiones futuras.
            /// </summary>
            [Required(ErrorMessage = "El código es obligatorio.")]
            public string Code { get; set; }

        }

        public IActionResult OnGet([FromQuery] string code = null, [FromQuery] string email = null)
        {
            // Obtener y mostrar la URL completa
            var fullUrl = Request.GetDisplayUrl();
            Console.WriteLine($"URL completa: {fullUrl}");

            // Verificar el código
            if (code == null)
            {
                return BadRequest("Se debe proporcionar un código para restablecer la contraseña.");
            }

            // Mostrar los valores recibidos para depuración
            Console.WriteLine($"Código recibido: {code}");

            // Comprobar si el email está presente en la URL
            if (string.IsNullOrEmpty(email))
            {
                // Si email es null o vacío, intenta capturarlo manualmente
                email = ExtractEmailFromUrl(fullUrl);
            }

            // Verificar si el email sigue vacío
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("El email está vacío o nulo.");
                return BadRequest("El email no se proporcionó.");
            }

            // Mostrar los valores recibidos para depuración
            Console.WriteLine($"Email recibido: {email}");

            // Decodificar el código
            string decodedCode;
            try
            {
                decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al decodificar el código: {ex.Message}");
                return BadRequest("El código proporcionado es inválido.");
            }

            // Crear el modelo de entrada
            Input = new InputModel
            {
                Code = decodedCode,
                Email = email // Usar el email que hemos capturado
            };

            return Page();
        }

        // Método para extraer el email de la URL
        private string ExtractEmailFromUrl(string url)
        {
            // Busca el inicio de 'email=' en la URL
            var emailIndex = url.IndexOf("email=");

            if (emailIndex >= 0) // Si se encuentra 'email='
            {
                // Extrae el substring desde 'email=' hasta el final de la URL
                var emailPart = url.Substring(emailIndex + 6); // +6 para saltar 'email='
                var ampIndex = emailPart.IndexOf("&"); // Encuentra el siguiente '&'

                // Si hay un '&', toma solo hasta ese punto; de lo contrario, toma el resto
                if (ampIndex > 0)
                {
                    return emailPart.Substring(0, ampIndex); // Devuelve el email
                }
                else
                {
                    return emailPart; // Devuelve todo si no hay otro parámetro
                }
            }

            return null; // Si no se encuentra 'email=', devuelve null
        }






        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}