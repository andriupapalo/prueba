using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class ListaMovContable
    {
        public int cantidad { get; set; }
        public bool valido { get; set; }
        public List<moviContable> movimientos {get;set;}
        public List<DocumentoDescuadradoModel> descuadrados { get; set; }

    }

    public class moviContable
    {
        public int idmov { get; set; }
        public int id_encab { get; set; }
        public int seq { get; set; }
        public int idparametronombre { get; set; }
        public int cuenta { get; set; }
        public int? tipotarifa { get; set; }
        public int centro { get; set; }
        public int nit { get; set; }
        public System.DateTime fec { get; set; }
        public decimal debito { get; set; }
        public decimal credito { get; set; }
        public decimal basecontable { get; set; }
        public decimal debitoniif { get; set; }
        public decimal creditoniif { get; set; }
        public string documento { get; set; }
        public string detalle { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<int> idterceroadmin { get; set; }
        public Nullable<int> tipo_tarifa { get; set; }
    }
}

    
