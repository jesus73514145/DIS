using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyecto.Models
{
    [Table("t_material")]
    public class Material
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }  // ID autogenerado para la tabla

        public string? UserID { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        public string Modelo { get; set; }  // Apartado solicitado

        [Required(ErrorMessage = "El nombre de la tela es obligatorio.")]
        public string NombreTela { get; set; }  // Nombre de la tela


        [Required(ErrorMessage = "El precio por metro es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public double? Precio { get; set; }  // Precio por metro de tela

        [Required(ErrorMessage = "La cantidad de metros es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int? Cantidad { get; set; }  // Cantidad de metros de tela

        [Required(ErrorMessage = "El precio total es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio total debe ser mayor a 0.")]
        public double? PrecioTotal { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        public string? Proveedor { get; set; }  // Nombre de la empresa

        [Required(ErrorMessage = "El contacto del proveedor es obligatorio.")]
        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        public string? ProveedorContacto { get; set; }  // Número de teléfono del proveedor

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        public DateTime FechaRegistro { get; set; }  // Día de ingreso

        public DateTime? FechaActualizacion { get; set; }  // Fecha de actualización (nullable)
    }
}