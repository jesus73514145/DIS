using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyecto.Models
{
    [Table("t_sedes")]
    public class Sedes
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sede es obligatorio.")]
        public string NombreSede { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public virtual ICollection<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
    }
}