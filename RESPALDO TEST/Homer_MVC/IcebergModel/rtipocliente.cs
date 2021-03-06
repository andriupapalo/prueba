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
    
    public partial class rtipocliente
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public rtipocliente()
        {
            this.icb_referencia_movdetalle = new HashSet<icb_referencia_movdetalle>();
            this.lineas_documento = new HashSet<lineas_documento>();
            this.lineas_documento1 = new HashSet<lineas_documento>();
            this.lineas_documento_operaciones = new HashSet<lineas_documento_operaciones>();
            this.mov_contable = new HashSet<mov_contable>();
            this.vpedrepuestos = new HashSet<vpedrepuestos>();
            this.tdetallemanoobraot = new HashSet<tdetallemanoobraot>();
            this.tdetallerepuestosot = new HashSet<tdetallerepuestosot>();
            this.ttrasladosorden = new HashSet<ttrasladosorden>();
        }
    
        public int id { get; set; }
        public string descripcion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public bool aplica_iva { get; set; }
        public bool aplica_descuento { get; set; }
        public bool cobra_cliente { get; set; }
        public bool selecciona_centro_costo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_referencia_movdetalle> icb_referencia_movdetalle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lineas_documento> lineas_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lineas_documento> lineas_documento1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lineas_documento_operaciones> lineas_documento_operaciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mov_contable> mov_contable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedrepuestos> vpedrepuestos { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallemanoobraot> tdetallemanoobraot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallerepuestosot> tdetallerepuestosot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ttrasladosorden> ttrasladosorden { get; set; }
    }
}
