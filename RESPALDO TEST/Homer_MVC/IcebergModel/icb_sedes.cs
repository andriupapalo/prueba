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
    
    public partial class icb_sedes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_sedes()
        {
            this.archivo_compra = new HashSet<archivo_compra>();
        }
    
        public int sede_id { get; set; }
        public string sede_codigo_bac { get; set; }
        public Nullable<int> sede_id_licencia { get; set; }
        public System.DateTime sede_fec_creacion { get; set; }
        public int sede_userid_creacion { get; set; }
        public Nullable<System.DateTime> sede_fec_actualizacion { get; set; }
        public Nullable<int> sede_userid_actualizacion { get; set; }
        public string sede_nombre { get; set; }
        public bool sede_estado { get; set; }
        public string sede_razon_inactivo { get; set; }
        public int sede_canal_distribucion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<archivo_compra> archivo_compra { get; set; }
        public virtual rtiposcanalesdistribucion rtiposcanalesdistribucion { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
