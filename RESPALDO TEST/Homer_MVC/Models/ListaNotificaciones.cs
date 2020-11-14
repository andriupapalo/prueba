using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class ListaNotificaciones
    {
        public bool leido { get; set; }
        public int id { get; set; }
        public string nombre { get; set; }
        public string placa { get; set; }
    }
}