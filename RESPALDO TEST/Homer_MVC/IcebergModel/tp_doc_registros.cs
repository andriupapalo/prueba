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
    
    public partial class tp_doc_registros
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tp_doc_registros()
        {
            this.cruce_documentos = new HashSet<cruce_documentos>();
            this.cruce_documentos1 = new HashSet<cruce_documentos>();
            this.detalle_anulacion = new HashSet<detalle_anulacion>();
            this.documentos_bodega = new HashSet<documentos_bodega>();
            this.encab_documento = new HashSet<encab_documento>();
            this.grupoconsecutivos = new HashSet<grupoconsecutivos>();
            this.icb_doc_consecutivos = new HashSet<icb_doc_consecutivos>();
            this.icb_referencia_mov = new HashSet<icb_referencia_mov>();
            this.medios_movtos = new HashSet<medios_movtos>();
            this.perfil_contable_documento = new HashSet<perfil_contable_documento>();
            this.resolucionfactura = new HashSet<resolucionfactura>();
            this.rordencompra = new HashSet<rordencompra>();
            this.tdetallemanoobraot = new HashSet<tdetallemanoobraot>();
            this.tencabcotizacion = new HashSet<tencabcotizacion>();
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.tencabezaorden_exh = new HashSet<tencabezaorden_exh>();
            this.tordentot = new HashSet<tordentot>();
            this.tp_doc_registros1 = new HashSet<tp_doc_registros>();
            this.tparametrocontable = new HashSet<tparametrocontable>();
            this.vpedrepuestos = new HashSet<vpedrepuestos>();
        }
    
        public int tpdoc_id { get; set; }
        public Nullable<int> sw { get; set; }
        public string prefijo { get; set; }
        public string tpdoc_nombre { get; set; }
        public Nullable<int> tpdocid_licencia { get; set; }
        public System.DateTime tpdocfec_creacion { get; set; }
        public int tpdocuserid_creacion { get; set; }
        public Nullable<System.DateTime> tpdocfec_actualizacion { get; set; }
        public Nullable<int> tpdocuserid_actualizacion { get; set; }
        public bool tpdoc_estado { get; set; }
        public string tpdocrazoninactivo { get; set; }
        public float retencion { get; set; }
        public decimal baseretencion { get; set; }
        public float retica { get; set; }
        public decimal baseica { get; set; }
        public float retiva { get; set; }
        public decimal baseiva { get; set; }
        public Nullable<int> bodega { get; set; }
        public string texto1 { get; set; }
        public string texto2 { get; set; }
        public string texto3 { get; set; }
        public string texto4 { get; set; }
        public Nullable<int> concepto1 { get; set; }
        public float ret1 { get; set; }
        public decimal baseret1 { get; set; }
        public float ret2 { get; set; }
        public decimal baseret2 { get; set; }
        public Nullable<int> concepto2 { get; set; }
        public Nullable<int> tipo { get; set; }
        public bool aplicaniff { get; set; }
        public bool consecano { get; set; }
        public bool consecmes { get; set; }
        public bool interno { get; set; }
        public Nullable<bool> entrada_salida { get; set; }
        public Nullable<int> doc_interno_asociado { get; set; }
        public Nullable<bool> habilitar { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cruce_documentos> cruce_documentos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cruce_documentos> cruce_documentos1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detalle_anulacion> detalle_anulacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<documentos_bodega> documentos_bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<encab_documento> encab_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<grupoconsecutivos> grupoconsecutivos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_doc_consecutivos> icb_doc_consecutivos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_referencia_mov> icb_referencia_mov { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<medios_movtos> medios_movtos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<perfil_contable_documento> perfil_contable_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<resolucionfactura> resolucionfactura { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rordencompra> rordencompra { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallemanoobraot> tdetallemanoobraot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabcotizacion> tencabcotizacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden_exh> tencabezaorden_exh { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tordentot> tordentot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tp_doc_registros> tp_doc_registros1 { get; set; }
        public virtual tp_doc_registros tp_doc_registros2 { get; set; }
        public virtual tp_doc_registros_tipo tp_doc_registros_tipo { get; set; }
        public virtual tp_doc_sw tp_doc_sw { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedrepuestos> vpedrepuestos { get; set; }
    }
}
