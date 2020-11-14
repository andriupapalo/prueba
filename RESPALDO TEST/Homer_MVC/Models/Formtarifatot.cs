using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class Formtarifatot
        {
        public int id { get; set; }
        public string  Nombretarifa { get; set; }
        public decimal  valor { get; set; }
        public bool Estado { get; set; }

        }
    }