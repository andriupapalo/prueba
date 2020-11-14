using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class SolicitudTemporal
    {
        public int idtemp { get; set; }
        public Nullable<int> dataBogeda { get; set; }
        public Nullable<int> dataCliente { get; set; }
        public Nullable<int> idkitaccesorios { get; set; }
        public Nullable<int> dataBogeda2 { get; set; }
        public string idreferencia { get; set; }
        public string referencia { get; set; }
        public Nullable<int> Stock { get; set; }
        public Nullable<int> idseparacion { get; set; }
    }
}