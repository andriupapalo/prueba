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
    
    public partial class aencab_auditoriainv
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public aencab_auditoriainv()
        {
            this.aconteos_auditoria = new HashSet<aconteos_auditoria>();
            this.apareja_auditoriainv = new HashSet<apareja_auditoriainv>();
            this.ubicaciones_asignadas_3 = new HashSet<ubicaciones_asignadas_3>();
            this.ubicaciones_asignadas = new HashSet<ubicaciones_asignadas>();
            this.ubicaciones_asignadas_2 = new HashSet<ubicaciones_asignadas_2>();
        }
    
        public int id { get; set; }
        public string descripcion { get; set; }
        public System.DateTime fecha_inicio_aInv { get; set; }
        public System.DateTime fecha_fin_aInv { get; set; }
        public int tipo_conteo { get; set; }
        public System.DateTime fecha_creacion { get; set; }
        public Nullable<int> id_usuario_creacion { get; set; }
        public Nullable<System.DateTime> fecha_actualizacion { get; set; }
        public Nullable<int> id_usuario_actualizacion { get; set; }
        public Nullable<bool> estado { get; set; }
        public Nullable<int> bodega { get; set; }
        public string areas { get; set; }
        public string ubicaciones { get; set; }
        public string estanterias { get; set; }
        public Nullable<bool> finalizado { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<aconteos_auditoria> aconteos_auditoria { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<apareja_auditoriainv> apareja_auditoriainv { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicaciones_asignadas_3> ubicaciones_asignadas_3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicaciones_asignadas> ubicaciones_asignadas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicaciones_asignadas_2> ubicaciones_asignadas_2 { get; set; }
        public virtual bodega_concesionario bodega_concesionario { get; set; }
    }
}
