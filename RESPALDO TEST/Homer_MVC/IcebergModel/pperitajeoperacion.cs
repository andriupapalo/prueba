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
    
    public partial class pperitajeoperacion
    {
        public int id { get; set; }
        public Nullable<int> idZona { get; set; }
        public Nullable<int> idPieza { get; set; }
        public string idOperacion { get; set; }
        public Nullable<decimal> valor { get; set; }
        public int idEncabPeritaje { get; set; }
        public int userid_creacion { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<int> userid_actualizacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
    
        public virtual icb_encabezado_insp_peritaje icb_encabezado_insp_peritaje { get; set; }
        public virtual icb_piezaperitaje icb_piezaperitaje { get; set; }
        public virtual icb_zonaperitaje icb_zonaperitaje { get; set; }
        public virtual ttempario ttempario { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}