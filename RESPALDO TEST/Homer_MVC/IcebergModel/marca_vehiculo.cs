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
    
    public partial class marca_vehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public marca_vehiculo()
        {
            this.icb_vehiculo = new HashSet<icb_vehiculo>();
            this.modelo_vehiculo = new HashSet<modelo_vehiculo>();
        }
    
        public int marcvh_id { get; set; }
        public Nullable<int> marcvhid_licencia { get; set; }
        public System.DateTime marcvhfec_creacion { get; set; }
        public int marcvhuserid_creacion { get; set; }
        public Nullable<System.DateTime> marcvhfec_actualizacion { get; set; }
        public Nullable<int> marcvhuserid_actualizacion { get; set; }
        public string marcvh_nombre { get; set; }
        public bool marcvh_estado { get; set; }
        public string marcvhrazoninactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo> icb_vehiculo { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<modelo_vehiculo> modelo_vehiculo { get; set; }
    }
}