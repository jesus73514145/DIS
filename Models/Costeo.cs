using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
namespace proyecto.Models
{
    [Table("t_costeo")]
    public class Costeo
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Empresa es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede tener más de {1} caracteres.")]
        public string Empresa { get; set; }

        [Required(ErrorMessage = "La cantidad de prendas es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de prendas debe ser mayor a 0.")]
        public int? Cantidad_Prendas { get; set; }

        [Required(ErrorMessage = "El nombre de la Tela 1 es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la tela 1 no puede tener más de {1} caracteres.")]
        public string Tela1_Nombre { get; set; }

        [Required(ErrorMessage = "El costo de la Tela 1 es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de la tela 1 debe ser mayor a 0.")]
        public double? Tela1_Costo { get; set; }

        [Required(ErrorMessage = "La cantidad de Tela 1 es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad de tela 1 debe ser mayor a 0.")]
        public double? Tela1_Cantidad { get; set; }

        [StringLength(100, ErrorMessage = "El nombre de la tela 2 no puede tener más de {1} caracteres.")]
        public string? Tela2_Nombre { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de la tela 2 debe ser mayor a 0.")]
        public double? Tela2_Costo { get; set; } // Campo que acepta nulos

        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad de tela 2 debe ser mayor a 0.")]
        public double? Tela2_Cantidad { get; set; } // Campo que acepta nulos

        [Required(ErrorMessage = "El costo del Molde es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo del molde debe ser mayor a 0.")]
        public double? Molde { get; set; }

        [Required(ErrorMessage = "El costo del Tizado es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo del tizado debe ser mayor a 0.")]
        public double? Tizado { get; set; }

        [Required(ErrorMessage = "El costo del Corte es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo del corte debe ser mayor a 0.")]
        public double? Corte { get; set; }

        [Required(ErrorMessage = "El costo de la Confección es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de la confección debe ser mayor a 0.")]
        public double? Confección { get; set; }

        [Required(ErrorMessage = "El costo de los Botones es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de los botones debe ser mayor a 0.")]
        public double? Botones { get; set; }

        [Required(ErrorMessage = "El costo del Pegado de Botón es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo del pegado de botón debe ser mayor a 0.")]
        public double? Pegado_Botón { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo de otros debe ser mayor o igual a 0.")]
        public double? Otros { get; set; } // Campo que acepta nulos

        [Required(ErrorMessage = "El costo de Avíos es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de avíos debe ser mayor a 0.")]
        public double? Avios { get; set; }

        [Required(ErrorMessage = "El costo de Tricotex es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de tricotex debe ser mayor a 0.")]
        public double? Tricotex { get; set; }

        [Required(ErrorMessage = "El costo de Acabados es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de acabados debe ser mayor a 0.")]
        public double? Acabados { get; set; }

        [Required(ErrorMessage = "El costo de Transporte es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo de transporte debe ser mayor a 0.")]
        public double? CostoTransporte { get; set; }


        public double? CU_Final { get; set; }


        public double? CT_Final { get; set; }

        // Campos que no se crearán en la base de datos, pero se validarán en la vista
        [NotMapped]
        [Required(ErrorMessage = "El precio de la tela 1 es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de la tela 1 debe ser mayor a 0.")]
        public double? Precio_Tela1 { get; set; }

        [NotMapped]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de la tela 2 debe ser mayor a 0.")]
        public double? Precio_Tela2 { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "El precio del transporte es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio del transporte debe ser mayor a 0.")]
        public double? Precio_Transporte { get; set; }

        public string? UserID { get; set; }

        [NotNull]
        public DateTime fec_Creacion { get; set; }

        public DateTime? fec_Actualizacion { get; set; }
    }
}