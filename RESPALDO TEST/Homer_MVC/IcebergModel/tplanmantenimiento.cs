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
    
    public partial class tplanmantenimiento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tplanmantenimiento()
        {
            this.insumosplanmantenimiento = new HashSet<insumosplanmantenimiento>();
            this.tcitarepuestos = new HashSet<tcitarepuestos>();
            this.tcitasoperacion = new HashSet<tcitasoperacion>();
            this.tcitastaller = new HashSet<tcitastaller>();
            this.tdetallemanoobraot = new HashSet<tdetallemanoobraot>();
            this.tdetallerepuestosot = new HashSet<tdetallerepuestosot>();
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.ttempario = new HashSet<ttempario>();
            this.tplanrepuestosmodelo = new HashSet<tplanrepuestosmodelo>();
        }
    
        public int id { get; set; }
        public string Descripcion { get; set; }
        public int kilometraje { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<insumosplanmantenimiento> insumosplanmantenimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitarepuestos> tcitarepuestos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitasoperacion> tcitasoperacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitastaller> tcitastaller { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallemanoobraot> tdetallemanoobraot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallerepuestosot> tdetallerepuestosot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ttempario> ttempario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tplanrepuestosmodelo> tplanrepuestosmodelo { get; set; }
    }
}
