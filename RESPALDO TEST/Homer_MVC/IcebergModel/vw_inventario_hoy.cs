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
    
    public partial class vw_inventario_hoy
    {
        public int id { get; set; }
        public int bodega { get; set; }
        public string nombreBodega { get; set; }
        public string ref_codigo { get; set; }
        public string ref_descripcion { get; set; }
        public Nullable<int> mes { get; set; }
        public Nullable<int> ano { get; set; }
        public decimal can_ini { get; set; }
        public decimal can_ent { get; set; }
        public decimal can_sal { get; set; }
        public decimal stock { get; set; }
        public string modulo { get; set; }
        public string clasificacion_ABC { get; set; }
        public Nullable<decimal> costo_prom { get; set; }
    }
}
