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
    
    public partial class color_vehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public color_vehiculo()
        {
            this.detallePedido_GM = new HashSet<detallePedido_GM>();
            this.icb_vehiculo = new HashSet<icb_vehiculo>();
            this.rseparacionmercancia = new HashSet<rseparacionmercancia>();
        }
    
        public string colvh_id { get; set; }
        public Nullable<int> colvhid_licencia { get; set; }
        public System.DateTime colvhfec_creacion { get; set; }
        public int colvhuserid_creacion { get; set; }
        public Nullable<System.DateTime> colvhfec_actualizacion { get; set; }
        public Nullable<int> colvhuserid_actualizacion { get; set; }
        public string colvh_nombre { get; set; }
        public bool colvh_estado { get; set; }
        public string colvhrazoninactivo { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detallePedido_GM> detallePedido_GM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo> icb_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rseparacionmercancia> rseparacionmercancia { get; set; }
    }
}
