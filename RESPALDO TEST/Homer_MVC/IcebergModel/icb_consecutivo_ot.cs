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
    
    public partial class icb_consecutivo_ot
    {
        public int otcon_id { get; set; }
        public string otcon_prefijo { get; set; }
        public Nullable<int> otcon_bodega { get; set; }
        public Nullable<int> otcon_consecutivo { get; set; }
        public Nullable<int> otcon_usuela { get; set; }
        public Nullable<System.DateTime> otcon_fecela { get; set; }
        public Nullable<int> otcon_usumod { get; set; }
        public Nullable<System.DateTime> otcon_fecmod { get; set; }
        public Nullable<bool> otcon_estado { get; set; }
        public string otcon_razoninactivo { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
