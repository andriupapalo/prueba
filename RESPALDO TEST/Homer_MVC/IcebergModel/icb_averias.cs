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
    
    public partial class icb_averias
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_averias()
        {
            this.icb_inspeccionvehiculos = new HashSet<icb_inspeccionvehiculos>();
        }
    
        public int ave_id { get; set; }
        public string ave_codigo { get; set; }
        public string ave_descripcion { get; set; }
        public Nullable<int> ave_licencia { get; set; }
        public System.DateTime ave_fec_creacion { get; set; }
        public int ave_userid_creacion { get; set; }
        public Nullable<System.DateTime> ave_fec_actualizacion { get; set; }
        public Nullable<int> ave_userid_actualizacion { get; set; }
        public bool ave_estado { get; set; }
        public string ave_razon_inactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_inspeccionvehiculos> icb_inspeccionvehiculos { get; set; }
    }
}