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
    
    public partial class vcotrepuestos
    {
        public int id { get; set; }
        public int cot_id { get; set; }
        public string referencia { get; set; }
        public Nullable<decimal> vrunitario { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public Nullable<bool> estado { get; set; }
        public string razon_inactivo { get; set; }
        public string modelo_id { get; set; }
        public Nullable<int> cantidad { get; set; }
    
        public virtual icb_cotizacion icb_cotizacion { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual modelo_vehiculo modelo_vehiculo { get; set; }
    }
}
