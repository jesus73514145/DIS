// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using proyecto.Models;

namespace proyecto.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMyEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IMyEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Por favor, introduce un correo electrónico válido.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Mostrar un mensaje de error que el correo no está registrado
                    ModelState.AddModelError(string.Empty, "No se encontró una cuenta con ese correo electrónico.");
                    return Page();
                }

                // Verificar si el correo está confirmado
                if (!(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Redirigir si el correo no está confirmado
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Generar token y enviar el correo
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = Input.Email },
                    protocol: Request.Scheme);

                // Mostrar la URL generada para depuración
                Console.WriteLine($"URL de restablecimiento de contraseña: {callbackUrl}");
                Console.WriteLine($"Email enviado: {Input.Email}");

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Restablecer la contraseña",
                    $"Por favor restablezca su contraseña <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='text-decoration:none; color:blue;'>haciendo clic aquí</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
