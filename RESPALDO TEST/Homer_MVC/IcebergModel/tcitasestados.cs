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
    
    public partial class tcitasestados
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tcitasestados()
        {
            this.tcitasestados1 = new HashSet<tcitasestados>();
            this.tcitastaller = new HashSet<tcitastaller>();
            this.tencabezaorden_exh = new HashSet<tencabezaorden_exh>();
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.ttrackingorden = new HashSet<ttrackingorden>();
        }
    
        public int id { get; set; }
        public string tipoestado { get; set; }
        public string Descripcion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string color_estado { get; set; }
        public int posicion { get; set; }
        public Nullable<int> estadoanterior { get; set; }
        public Nullable<int> ordenot { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitasestados> tcitasestados1 { get; set; }
        public virtual tcitasestados tcitasestados2 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitastaller> tcitastaller { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden_exh> tencabezaorden_exh { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ttrackingorden> ttrackingorden { get; set; }
    }
}
