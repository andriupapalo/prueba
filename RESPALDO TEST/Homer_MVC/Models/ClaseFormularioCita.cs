using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class ClaseFormularioCita
    {
    }

    public class listaOperacionesSinPlan
    {
        public string codigo { get; set; }
        public string operacion { get; set; }
        public decimal tiempo { get; set; }
        public decimal precio { get; set; }
        public decimal ivaOperacion { get; set; }
        public decimal valorIva { get; set; }
        public decimal valorTotal { get; set; }
    }
}