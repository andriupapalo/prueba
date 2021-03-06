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
    
    public partial class icb_cuadre_caja
    {
        public int id { get; set; }
        public Nullable<int> caja { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public Nullable<int> estado { get; set; }
        public Nullable<bool> cerrada { get; set; }
        public Nullable<int> bodega { get; set; }
        public Nullable<int> responsable { get; set; }
        public Nullable<decimal> efectivo { get; set; }
        public Nullable<decimal> sistema_efectivo { get; set; }
        public Nullable<decimal> diferencia_efectivo { get; set; }
        public Nullable<decimal> tarjetas { get; set; }
        public Nullable<decimal> sistema_tarjetas { get; set; }
        public Nullable<decimal> diferencia_tarjetas { get; set; }
        public Nullable<decimal> cheques { get; set; }
        public Nullable<decimal> sistema_cheques { get; set; }
        public Nullable<decimal> diferencia_cheques { get; set; }
        public Nullable<decimal> recibos { get; set; }
        public Nullable<decimal> sistema_recibos { get; set; }
        public Nullable<decimal> diferencia_recibos { get; set; }
        public Nullable<decimal> total_ingresos { get; set; }
        public Nullable<decimal> total_egresos { get; set; }
        public Nullable<decimal> total_sistema { get; set; }
        public Nullable<decimal> total { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual icb_caja icb_caja { get; set; }
        public virtual icb_estado_caja icb_estado_caja { get; set; }
        public virtual users users { get; set; }
    }
}
