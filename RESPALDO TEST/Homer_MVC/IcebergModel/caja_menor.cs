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
    
    public partial class caja_menor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public caja_menor()
        {
            this.caja_menor_bodega = new HashSet<caja_menor_bodega>();
            this.caja_menor_encab = new HashSet<caja_menor_encab>();
        }
    
        public int cjm_id { get; set; }
        public Nullable<int> id_responsable { get; set; }
        public string cjm_desc { get; set; }
        public Nullable<decimal> cjm_valor { get; set; }
        public Nullable<int> cjm_usuela { get; set; }
        public Nullable<System.DateTime> cjm_fecela { get; set; }
        public Nullable<int> cjm_usumod { get; set; }
        public Nullable<System.DateTime> cjm_fecmod { get; set; }
        public Nullable<bool> cjm_estado { get; set; }
        public string cjm_razoninactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<caja_menor_bodega> caja_menor_bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<caja_menor_encab> caja_menor_encab { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
    }
}
