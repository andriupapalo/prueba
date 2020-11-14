using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class Formestadogarantia
        {

        public int id { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public int user_creacion { get; set; }
        public string razoninactivo { get; set; }
        public int EstadoDependencia { get; set; }



        }
    }