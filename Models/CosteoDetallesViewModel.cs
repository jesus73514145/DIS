using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto.Models
{
    public class CosteoDetallesViewModel
    {
        public int Id { get; set; }
        public string Empresa { get; set; }
        public double? CU_Final { get; set; }
        public double? CT_Final { get; set; }
        public double? Tela1_Costo { get; set; }
        public double? Tela2_Costo { get; set; }
        public double? SumaCostos { get; set; }
        public double? CostoTransporte { get; set; }
    }
}