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
    
    public partial class icb_statusprospecto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_statusprospecto()
        {
            this.icb_registro_llamadas = new HashSet<icb_registro_llamadas>();
        }
    
        public int status_id { get; set; }
        public string status_nombre { get; set; }
        public System.DateTime status_fecela { get; set; }
        public Nullable<System.DateTime> status_fecha_actualizacion { get; set; }
        public int status_usuela { get; set; }
        public Nullable<int> status_usuario_actualizacion { get; set; }
        public Nullable<int> status_id_licencia { get; set; }
        public bool status_estado { get; set; }
        public string status_razon_inactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_registro_llamadas> icb_registro_llamadas { get; set; }
    }
}