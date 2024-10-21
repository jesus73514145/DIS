using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyecto.Models
{
    [Table("t_prenda")]
    public class Prenda
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }  // ID autogenerado para la tabla

        public string? UserID { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        public string Modelo { get; set; }  // Apartado solicitado

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        public string Cliente { get; set; }  // Nombre de la tela

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public double? PrecioUnitario { get; set; }  // Precio por metro de tela

        [Required(ErrorMessage = "La cantidad de la prenda es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int? Cantidad { get; set; }  // Cantidad de metros de tela

        [Required(ErrorMessage = "El costo total es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo total debe ser mayor a 0.")]
        public double? CostoTotal { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        public DateTime FechaRegistro { get; set; }  // Día de ingreso

        public DateTime? FechaActualizacion { get; set; }  // Fecha de actualización (nullable)
    }
}