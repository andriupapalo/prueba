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
    
    public partial class tot_combo
    {
        public int id { get; set; }
        public Nullable<int> numot { get; set; }
        public string idtempario { get; set; }
        public Nullable<bool> estado { get; set; }
        public Nullable<System.DateTime> fechacreacion { get; set; }
        public Nullable<int> usercreacion { get; set; }
        public Nullable<int> tipotarifa { get; set; }
        public Nullable<decimal> totalHorasCliente { get; set; }
        public Nullable<decimal> totalhorasoperario { get; set; }
        public Nullable<decimal> valortotal { get; set; }
    
        public virtual tencabezaorden tencabezaorden { get; set; }
        public virtual ttempario ttempario { get; set; }
        public virtual ttipostarifa ttipostarifa { get; set; }
        public virtual users users { get; set; }
    }
}
