using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace proyecto.Models.DTO
{

    public class UsuarioDTO
    {
        public string Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPat { get; set; }
        public string ApellidoMat { get; set; }
        public string Email { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public bool Activo { get; set; }
        public string Celular {get; set;}
        public string Genero {get; set;}
    }

}

