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
    
    public partial class icb_impacto_averia
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_impacto_averia()
        {
            this.icb_inspeccionvehiculos = new HashSet<icb_inspeccionvehiculos>();
        }
    
        public int impacto_id { get; set; }
        public Nullable<int> impacto_licencia { get; set; }
        public System.DateTime impacto_fec_creacion { get; set; }
        public int impacto_userid_creacion { get; set; }
        public Nullable<System.DateTime> impacto_fec_actualizacion { get; set; }
        public Nullable<int> impacto_userid_actualizacion { get; set; }
        public string impacto_descripcion { get; set; }
        public bool impacto_estado { get; set; }
        public string impacto_razon_inactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_inspeccionvehiculos> icb_inspeccionvehiculos { get; set; }
    }
}
