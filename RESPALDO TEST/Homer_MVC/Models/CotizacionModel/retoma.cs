using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models.CotizacionModel
{
    public partial  class retoma
    {
        public int ano { get; set; }
        public int idcot { get; set; }
        public Nullable<int> Kilometraje { get; set; }
        public string modelo { get; set; }
        public string placa { get; set; }
        public decimal valor { get; set; }
    }
}