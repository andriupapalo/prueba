using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class SalidaPDF
    {
        public string nombreDetalle { get; set; }
        public long numeroRegistro { get; set; }
        public string fechaRegistro { get; set; }
        public string nombreBodega { get; set; }
        public int secuencia { get; set; }
        public string numeroRefencia { get; set; }
        public string descripcion { get; set; }
        public decimal cantidad { get; set; }
        public decimal costoUnidad { get; set; }
        public decimal costoTotal { get; set; }

        public decimal TotalTotales { get; set; }



        public string notas { get; set; }

        public List<referenciasPDF> referencias { get; set; }
    }

    public class referenciasSalidaPDF
    {
        public decimal cantidad { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal costo { get; set; }
        public decimal total { get; set; }
        public int secuencia { get; set; }



    }
}

