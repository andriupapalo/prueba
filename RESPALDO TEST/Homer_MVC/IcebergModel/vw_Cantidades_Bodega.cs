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
    
    public partial class vw_Cantidades_Bodega
    {
        public int idencabezado { get; set; }
        public int bodega { get; set; }
        public string bodccs_nombre { get; set; }
        public decimal valor_total { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<decimal> suma { get; set; }
        public Nullable<decimal> sumavalor { get; set; }
        public Nullable<decimal> cantidad_accesorios { get; set; }
        public Nullable<decimal> cantidad_repuestos { get; set; }
        public Nullable<decimal> costo_accesorios { get; set; }
        public Nullable<decimal> costo_repuestos { get; set; }
    }
}