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
    
    public partial class clasificacion_repuesto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public clasificacion_repuesto()
        {
            this.icb_referencia = new HashSet<icb_referencia>();
        }
    
        public int clarpto_id { get; set; }
        public Nullable<int> clarptoid_licencia { get; set; }
        public System.DateTime clarptofec_creacion { get; set; }
        public int clarptouserid_creacion { get; set; }
        public Nullable<System.DateTime> clarptofec_actualizacion { get; set; }
        public Nullable<int> clarptouserid_actualizacion { get; set; }
        public string clarpto_nombre { get; set; }
        public bool clarpto_estado { get; set; }
        public string clarptorazoninactivo { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_referencia> icb_referencia { get; set; }
    }
}
