using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using proyecto.Models.DTO;

namespace proyecto.Models.Validator
{
    public class UsuarioValidator : AbstractValidator<UsuarioDTO>
    {
        public UsuarioValidator()
        {
            RuleFor(usuario => usuario.Nombres)
           .NotEmpty().WithMessage("El campo de Nombre es obligatorio");

           RuleFor(usuario => usuario.ApellidoPat)
            .NotEmpty().WithMessage("El campo de Apellido Paterno es obligatorio");

            RuleFor(usuario => usuario.ApellidoMat)
            .NotEmpty().WithMessage("El campo de Apellido Materno es obligatorio");

            RuleFor(usuario => usuario.Email)
            .NotEmpty().WithMessage("El campo de Email es obligatorio");

            RuleFor(usuario => usuario.NumeroDocumento)
            .NotEmpty().WithMessage("El campo de Numero de Documento es obligatorio")
            .NotNull().WithMessage("El campo de Numero de Documento es obligatorio");

            RuleFor(usuario => usuario.Celular)
            .NotEmpty().WithMessage("El campo de Celular es obligatorio");

            RuleFor(usuario => usuario.Genero)
            .NotNull().WithMessage("El campo de genero no puede ser vacio, seleccione uno");
        }

    }
}