//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Homer_MVC.IcebergModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class vw_browserBackOffice_VPedidosAnulados
    {
        public int id { get; set; }
        public Nullable<int> numero { get; set; }
        public string modelo { get; set; }
        public string proceso { get; set; }
        public Nullable<decimal> vrtotal { get; set; }
        public Nullable<decimal> valor { get; set; }
        public Nullable<decimal> saldo { get; set; }
        public Nullable<int> idCliente { get; set; }
        public Nullable<int> tpdoc_id { get; set; }
        public string tipo { get; set; }
        public string doc_tercero { get; set; }
        public bool solicitado { get; set; }
        public bool para_facturar { get; set; }
        public string razon_social { get; set; }
        public string prinom_tercero { get; set; }
        public string segnom_tercero { get; set; }
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }
        public Nullable<int> idAsesor { get; set; }
        public Nullable<int> user_numIdent { get; set; }
        public string asesor { get; set; }
        public System.DateTime fecha { get; set; }
        public bool facturado { get; set; }
        public bool anulado { get; set; }
        public Nullable<int> numfactura { get; set; }
        public string planmayor { get; set; }
        public string vin { get; set; }
        public string plac_vh { get; set; }
        public Nullable<int> anio_vh { get; set; }
        public Nullable<System.DateTime> fecmatricula { get; set; }
        public string ubivh_nombre { get; set; }
        public string colvh_nombre { get; set; }
        public Nullable<System.DateTime> fecha_asignacion_planmayor { get; set; }
        public int bodega { get; set; }
        public string bodccs_nombre { get; set; }
        public string codigo { get; set; }
        public Nullable<bool> autorizado { get; set; }
        public Nullable<System.DateTime> fecha_autorizacion { get; set; }
        public string autorizados { get; set; }
        public Nullable<System.DateTime> fecha_entrega { get; set; }
        public Nullable<System.DateTime> fecha_venta { get; set; }
    }
}
