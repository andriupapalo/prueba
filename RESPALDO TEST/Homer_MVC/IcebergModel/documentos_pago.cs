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
    
    public partial class documentos_pago
    {
        public int idpago { get; set; }
        public int idtencabezado { get; set; }
        public Nullable<int> pedido { get; set; }
        public Nullable<int> banco { get; set; }
        public string documento { get; set; }
        public int forma_pago { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public decimal valor { get; set; }
        public Nullable<int> consignar_en { get; set; }
        public string devuelto { get; set; }
        public string tipo_consignacion { get; set; }
        public Nullable<int> numero_consignacion { get; set; }
        public Nullable<System.DateTime> fecha_devolucion { get; set; }
        public string cuenta_banco { get; set; }
        public Nullable<decimal> iva_tarjeta { get; set; }
        public string notas { get; set; }
        public Nullable<int> tipo_devuelto { get; set; }
        public Nullable<int> numero_devuelto { get; set; }
        public int tercero { get; set; }
    
        public virtual bancos bancos { get; set; }
        public virtual bancos bancos1 { get; set; }
        public virtual encab_documento encab_documento { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual tipopagorecibido tipopagorecibido { get; set; }
    }
}
