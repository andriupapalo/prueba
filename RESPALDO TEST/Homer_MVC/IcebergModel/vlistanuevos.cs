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
    
    public partial class vlistanuevos
    {
        public int id { get; set; }
        public int lista { get; set; }
        public int ano { get; set; }
        public int mes { get; set; }
        public int anomodelo { get; set; }
        public string concepto { get; set; }
        public string descripcion { get; set; }
        public string planmayor { get; set; }
        public decimal preciolista { get; set; }
        public decimal precioespecial { get; set; }
        public Nullable<decimal> descuento { get; set; }
        public Nullable<float> porcendescuento { get; set; }
    
        public virtual anio_modelo anio_modelo { get; set; }
    }
}
