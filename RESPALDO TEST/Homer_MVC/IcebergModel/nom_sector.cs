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
    
    public partial class nom_sector
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public nom_sector()
        {
            this.terceros_direcciones = new HashSet<terceros_direcciones>();
        }
    
        public int sec_id { get; set; }
        public string sec_nombre { get; set; }
        public Nullable<int> secid_licencia { get; set; }
        public Nullable<System.DateTime> secfec_creacion { get; set; }
        public Nullable<System.DateTime> secfec_actualizacion { get; set; }
        public Nullable<int> secuserid_creacion { get; set; }
        public Nullable<int> secuserid_actualizacion { get; set; }
        public bool sec_estado { get; set; }
        public string sec_razoninactivo { get; set; }
        public Nullable<int> ciudad_id { get; set; }
    
        public virtual nom_ciudad nom_ciudad { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<terceros_direcciones> terceros_direcciones { get; set; }
    }
}