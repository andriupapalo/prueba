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
    
    public partial class vw_Datos_lealtad
    {
        public int idencabezado { get; set; }
        public int nit { get; set; }
        public string Nombre_proveedor { get; set; }
        public int bodega { get; set; }
        public string bodccs_nombre { get; set; }
        public Nullable<decimal> cantidadref { get; set; }
        public Nullable<decimal> cantidadaccesorios { get; set; }
        public Nullable<decimal> cantidadrefrepuestos { get; set; }
        public Nullable<decimal> costoaccesorios { get; set; }
        public Nullable<decimal> costorepuestos { get; set; }
        public Nullable<int> tipo_proveedor { get; set; }
        public string nombre { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public decimal valor_total { get; set; }
        public Nullable<decimal> valor_documento { get; set; }
        public decimal valor_mercancia { get; set; }
        public Nullable<int> Clasificacion_pro { get; set; }
        public string descripcion { get; set; }
    }
}