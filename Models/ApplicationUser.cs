using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;


namespace proyecto.Models {
    public class ApplicationUser : IdentityUser {

        [Required] // Esto asegura que el campo no puede ser null
        public string? Nombres { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public string? ApellidoPat { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public string? ApellidoMat { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public string? Dni { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public string? Celular { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public string? Genero { get; set; }

        [Required] // Esto asegura que el campo no puede ser null
        public DateTime? fechaDeRegistro { get; set; }

        public DateTime? fechaDeActualizacion { get; set; }
    }
    
}