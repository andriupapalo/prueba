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
    
    public partial class caja_menor_bodega
    {
        public int cjmb_id { get; set; }
        public Nullable<int> cjm_id { get; set; }
        public Nullable<int> id_bodega { get; set; }
        public Nullable<int> cjmb_usuela { get; set; }
        public Nullable<System.DateTime> cjmb_fecela { get; set; }
        public Nullable<int> cjmb_usumod { get; set; }
        public Nullable<System.DateTime> cjmb_fecmod { get; set; }
    
        public virtual caja_menor caja_menor { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}