using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class rsepacionMercanciaForm
    {
        /*clase form*/
        public int id { get; set; }
        public int bodega { get; set; }
        public string codigo { get; set; }
        public Nullable<int> cliente { get; set; }
        public System.DateTime fecha { get; set; }
        public int cantidad { get; set; }
        public Nullable<int> idordentaller { get; set; }
        public Nullable<int> idcita { get; set; }
        public Nullable<int> idpedido { get; set; }
        public string notas { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string placa { get; set; }
        public string color { get; set; }
        public Nullable<System.DateTime> fechaFinal { get; set; }
        public Nullable<int> separacion { get; set; }

    }
}