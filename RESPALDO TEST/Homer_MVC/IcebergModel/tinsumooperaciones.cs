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
    
    public partial class tinsumooperaciones
    {
        public int id { get; set; }
        public Nullable<int> numot { get; set; }
        public Nullable<int> codigo_insumo { get; set; }
        public Nullable<int> tipotarifa { get; set; }
        public Nullable<decimal> porcentaje_total_ { get; set; }
        public Nullable<decimal> valort_horas { get; set; }
        public Nullable<decimal> valortarifa { get; set; }
    
        public virtual tencabezaorden tencabezaorden { get; set; }
        public virtual Tinsumo Tinsumo { get; set; }
        public virtual ttipostarifa ttipostarifa { get; set; }
    }
}
