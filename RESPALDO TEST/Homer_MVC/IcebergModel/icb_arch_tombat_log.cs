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
    
    public partial class icb_arch_tombat_log
    {
        public int tombat_log_id { get; set; }
        public string tombat_log_licencia { get; set; }
        public string tombat_log_nombrearchivo { get; set; }
        public Nullable<int> tombat_log_items { get; set; }
        public Nullable<int> tombat_log_itemscorrecto { get; set; }
        public Nullable<int> tombat_log_itemserror { get; set; }
        public Nullable<System.DateTime> tombat_log_fecha { get; set; }
        public string tombat_log_log { get; set; }
        public Nullable<int> id_arch_tombat { get; set; }
    
        public virtual icb_arch_tombat icb_arch_tombat { get; set; }
    }
}
