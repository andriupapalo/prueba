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
    
    public partial class ttrasladosorden
    {
        public int id { get; set; }
        public int idorden { get; set; }
        public int idtraslado { get; set; }
        public int idtercero { get; set; }
        public Nullable<int> idtipotarifa { get; set; }
        public string codigo { get; set; }
        public int cantidad { get; set; }
        public decimal preciounitario { get; set; }
        public decimal poriva { get; set; }
        public decimal pordescuento { get; set; }
        public decimal costopromedio { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<int> idcentro { get; set; }
    
        public virtual centro_costo centro_costo { get; set; }
        public virtual encab_documento encab_documento { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual rtipocliente rtipocliente { get; set; }
        public virtual tencabezaorden tencabezaorden { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
