using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class MedioPago
    {

        public int? idEncabezado { get; set; }
        public int? idorden { get; set; }
        public int? medioPago { get; set; }
        public string valor { get; set; }
        public string total { get; set; }
        public string vaucher { get; set; }
        public string Cheque { get; set; }

    }
}