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
    
    public partial class seguimiento_admin_vh
    {
        public int id { get; set; }
        public string planmayor { get; set; }
        public string observacion { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<int> userid_creacion { get; set; }
    
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual users users { get; set; }
    }
}