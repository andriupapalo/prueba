using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class PedidoSugeridoModel
    {

        public string Referencia { get; set; }
        public string Clasificacion { get; set; }
        public string bodega { get; set; }
        public string codigo { get; set; }
        public decimal cantidad { get; set; }
        public List<FechasPedidoSugerido> ValoresPorFecha { get; set; }

        public PedidoSugeridoModel()
        {
            ValoresPorFecha = new List<FechasPedidoSugerido>();
        }


    }

}