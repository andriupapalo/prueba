using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class TOTformulario
        {
        public int id { get; set; }
        public int? tipo { get; set; }
        public int perfil { get; set; }
        public int Bodega { get; set; }
        public int numerofactura { get; set; }
        public DateTime fecha { get; set; }
        public int fpago { get; set; }
        public int numot { get; set; }
        public int tipotarifa { get; set; }
        public int proveedor { get; set; }
        }
    }