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
    
    public partial class ttipooperacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ttipooperacion()
        {
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.tencabezaorden_exh = new HashSet<tencabezaorden_exh>();
            this.tparametrocontable = new HashSet<tparametrocontable>();
            this.ttempario = new HashSet<ttempario>();
        }
    
        public int id { get; set; }
        public string Descripcion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden_exh> tencabezaorden_exh { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ttempario> ttempario { get; set; }
    }
}
