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
    
    public partial class acteco_tercero
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public acteco_tercero()
        {
            this.tercero_proveedor = new HashSet<tercero_proveedor>();
            this.icb_terceros = new HashSet<icb_terceros>();
            this.tercero_cliente = new HashSet<tercero_cliente>();
            this.terceros_bod_ica = new HashSet<terceros_bod_ica>();
        }
    
        public int acteco_id { get; set; }
        public string nroacteco_tercero { get; set; }
        public string acteco_nombre { get; set; }
        public bool acteco_estado { get; set; }
        public Nullable<int> actecoid_licencia { get; set; }
        public System.DateTime actecofec_creacion { get; set; }
        public int actecouserid_creacion { get; set; }
        public Nullable<System.DateTime> actecofec_actualizacion { get; set; }
        public Nullable<int> actecouserid_actualizacion { get; set; }
        public string acteco_razoninactivo { get; set; }
        public decimal tarifa { get; set; }
        public decimal autorretencion { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tercero_proveedor> tercero_proveedor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_terceros> icb_terceros { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tercero_cliente> tercero_cliente { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<terceros_bod_ica> terceros_bod_ica { get; set; }
    }
}
