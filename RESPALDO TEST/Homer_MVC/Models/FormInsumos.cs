using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class FormInsumos
        {

        public int id { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal horas_insumo { get; set; }
        public decimal porcentaje { get; set; }
        public bool estado { get; set; }
        public string  razoninactivo { get; set; }
        }
    }