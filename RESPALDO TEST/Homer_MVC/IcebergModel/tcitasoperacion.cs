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
    
    public partial class tcitasoperacion
    {
        public int id { get; set; }
        public int idcita { get; set; }
        public string operacion { get; set; }
        public Nullable<decimal> tiempo { get; set; }
        public Nullable<int> idcentro { get; set; }
        public bool inspeccion { get; set; }
        public Nullable<int> id_plan_mantenimiento { get; set; }
    
        public virtual centro_costo centro_costo { get; set; }
        public virtual tcitastaller tcitastaller { get; set; }
        public virtual tplanmantenimiento tplanmantenimiento { get; set; }
        public virtual ttempario ttempario { get; set; }
    }
}
