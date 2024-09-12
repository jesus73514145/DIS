using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace proyecto.Models.DTO
{

    public class UsuarioEditDTO
    {
        public string Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Celular {get; set;}
        public string Genero {get; set;}
    }

}