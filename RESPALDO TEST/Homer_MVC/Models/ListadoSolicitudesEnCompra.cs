using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class ListadoSolicitudesEnCompra
    {
        public int solicitud { get; set; }
        public string pedidogm { get; set; }
        public string referencia { get; set; }
        public int cantidad { get; set; }
        public bool todo { get; set; }
    }
}