using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyecto.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string? Nombres { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
        public string? ApellidoPat { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
        public string? ApellidoMat { get; set; }

        [Required(ErrorMessage = "El Tipo de documento es obligatorio.")]
        [Display(Name = "Tipo de Documento")]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [Display(Name = "Numero de Documento")]
        public string? NumeroDocumento { get; set; }


        [Required(ErrorMessage = "El celular es obligatorio.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de celular debe tener 9 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El número de celular solo puede contener números.")]
        public string? Celular { get; set; }

        [Required(ErrorMessage = "El género es obligatorio.")]
        public string? Genero { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        public DateTime? fechaDeRegistro { get; set; }

        public DateTime? fechaDeActualizacion { get; set; }

        // Nuevo campo Estado como booleano
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; } // true: Activo, false: Inactivo

        // Campo para el rol del usuario como texto
        [Required(ErrorMessage = "El rol es obligatorio.")]
        [StringLength(20, ErrorMessage = "El rol no puede exceder los 20 caracteres.")]
        public string? RolId { get; set; } // Nombre del rol, e.g., "Admin"


        // Método de validación personalizado

    }

}