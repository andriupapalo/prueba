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
    
    public partial class vw_recalculoInventario
    {
        public Nullable<int> idencabezado { get; set; }
        public int bodega { get; set; }
        public Nullable<int> bodegaOrigen { get; set; }
        public Nullable<int> bodegaDestino { get; set; }
        public string bodccs_nombre { get; set; }
        public string codigo { get; set; }
        public string ref_descripcion { get; set; }
        public string modulo { get; set; }
        public decimal cantidad { get; set; }
        public decimal valor_unitario { get; set; }
        public Nullable<decimal> valorTotal { get; set; }
        public decimal costo_unitario { get; set; }
        public Nullable<decimal> costoTotal { get; set; }
        public Nullable<int> tipo { get; set; }
        public Nullable<int> sw { get; set; }
        public Nullable<long> numero { get; set; }
        public Nullable<int> anio { get; set; }
        public Nullable<int> mes { get; set; }
    }
}