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
    
    public partial class detalle_comision_financiera
    {
        public int iddetalle { get; set; }
        public int encabezado_id { get; set; }
        public Nullable<int> seq { get; set; }
        public string descripcion { get; set; }
        public Nullable<float> Porcen_iva { get; set; }
        public Nullable<float> porcen_descuento { get; set; }
        public Nullable<decimal> valor_unitario { get; set; }
        public Nullable<decimal> valor_total { get; set; }
    
        public virtual encab_documento encab_documento { get; set; }
    }
}
