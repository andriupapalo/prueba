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
    
    public partial class bodega_repuesto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bodega_repuesto()
        {
            this.agenda_mensajero = new HashSet<agenda_mensajero>();
            this.agenda_mensajero1 = new HashSet<agenda_mensajero>();
        }
    
        public int bodrpto_id { get; set; }
        public string bodrpto_nombre { get; set; }
        public Nullable<int> bodrptoid_licencia { get; set; }
        public System.DateTime bodrptofec_creacion { get; set; }
        public int bodrptouserid_creacion { get; set; }
        public Nullable<System.DateTime> bodrptofec_actualizacion { get; set; }
        public Nullable<int> bodrptouserid_actualizacion { get; set; }
        public bool bodrpto_estado { get; set; }
        public string bodrptorazoninactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_mensajero> agenda_mensajero { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_mensajero> agenda_mensajero1 { get; set; }
    }
}