using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering; // Añadir esta línea
namespace proyecto.Models
{
    public class UserEditViewModel
    
    {
        public string Id { get; set; }


        [Required(ErrorMessage = "El campo de correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo Nombres es obligatorio.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio.")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPat { get; set; }

        [Required(ErrorMessage = "El campo Apellido Materno es obligatorio.")]
        [Display(Name = "Apellido Materno")]
        public string ApellidoMat { get; set; }

        [Required(ErrorMessage = "El Tipo de Documento es obligatorio.")]
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

        [Required]
        public bool Activo { get; set; }

        [Required(ErrorMessage = "Por favor, selecciona una sede.")]
        [Display(Name = "Sede")]
        public int SedeId { get; set; }

        public List<SelectListItem>? Sedes { get; set; }

    }
}