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
    
    public partial class icb_detalle_plan
    {
        public int detplan_id { get; set; }
        public Nullable<int> detplan_usuela { get; set; }
        public Nullable<System.DateTime> detplan_fecela { get; set; }
        public string detplan_modelo_vehiculo { get; set; }
        public string detplan_descripcion { get; set; }
        public Nullable<int> detplan_plan_id { get; set; }
    
        public virtual icb_plan_financiero icb_plan_financiero { get; set; }
    }
}