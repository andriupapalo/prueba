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
    
    public partial class cuenta_puc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cuenta_puc()
        {
            this.activoclasificacion = new HashSet<activoclasificacion>();
            this.activoclasificacion1 = new HashSet<activoclasificacion>();
            this.amortizacionclasificacion = new HashSet<amortizacionclasificacion>();
            this.amortizacionclasificacion1 = new HashSet<amortizacionclasificacion>();
            this.cuenta_puc1 = new HashSet<cuenta_puc>();
            this.cuentas_valores = new HashSet<cuentas_valores>();
            this.mov_contable = new HashSet<mov_contable>();
            this.perfil_cuentas_documento = new HashSet<perfil_cuentas_documento>();
            this.retencionesbodega = new HashSet<retencionesbodega>();
            this.retencionesbodega1 = new HashSet<retencionesbodega>();
            this.retencionesbodega2 = new HashSet<retencionesbodega>();
            this.retencionesbodega3 = new HashSet<retencionesbodega>();
            this.retencionesbodega4 = new HashSet<retencionesbodega>();
            this.tablaempresa = new HashSet<tablaempresa>();
            this.tablaempresa1 = new HashSet<tablaempresa>();
            this.tablaempresa2 = new HashSet<tablaempresa>();
            this.tablaretenciones = new HashSet<tablaretenciones>();
            this.tablaretenciones1 = new HashSet<tablaretenciones>();
            this.tablaretenciones2 = new HashSet<tablaretenciones>();
            this.tablaretenciones3 = new HashSet<tablaretenciones>();
            this.tablaretenciones4 = new HashSet<tablaretenciones>();
            this.tparametrocontable = new HashSet<tparametrocontable>();
            this.tparametrocontable1 = new HashSet<tparametrocontable>();
            this.tparametrocontable2 = new HashSet<tparametrocontable>();
            this.tparametrocontable3 = new HashSet<tparametrocontable>();
            this.tparametrocontable4 = new HashSet<tparametrocontable>();
            this.tparametrocontable5 = new HashSet<tparametrocontable>();
        }
    
        public int cntpuc_id { get; set; }
        public string cntpuc_numero { get; set; }
        public string cntpuc_descp { get; set; }
        public string mov_cnt { get; set; }
        public bool cntpuc_estado { get; set; }
        public bool esafectable { get; set; }
        public bool ccostos { get; set; }
        public bool tercero { get; set; }
        public bool documeto { get; set; }
        public bool manejabase { get; set; }
        public string partida { get; set; }
        public string ctareversion { get; set; }
        public Nullable<decimal> porcentaje { get; set; }
        public Nullable<int> concepniff { get; set; }
        public Nullable<int> cuentaniff { get; set; }
        public string nombreniff { get; set; }
        public Nullable<int> cntpucid_licencia { get; set; }
        public System.DateTime cntpucfec_creacion { get; set; }
        public int cntpucuserid_creacion { get; set; }
        public Nullable<System.DateTime> cntpucfec_actualizacion { get; set; }
        public Nullable<int> cntpucuserid_actualizacion { get; set; }
        public string cntpucrazoninactivo { get; set; }
        public bool cuentacartera { get; set; }
        public bool cuentaproveedor { get; set; }
        public bool cuentaimpuestos { get; set; }
        public Nullable<bool> balance { get; set; }
        public Nullable<bool> estadoresultado { get; set; }
        public Nullable<bool> deorden { get; set; }
        public string clase { get; set; }
        public string grupo { get; set; }
        public string cuenta { get; set; }
        public string subcuenta { get; set; }
        public bool terceroadministrativo { get; set; }
        public bool flujodecaja { get; set; }
        public bool cuentapresupuesto { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<activoclasificacion> activoclasificacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<activoclasificacion> activoclasificacion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<amortizacionclasificacion> amortizacionclasificacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<amortizacionclasificacion> amortizacionclasificacion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cuenta_puc> cuenta_puc1 { get; set; }
        public virtual cuenta_puc cuenta_puc2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cuentas_valores> cuentas_valores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mov_contable> mov_contable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<perfil_cuentas_documento> perfil_cuentas_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaempresa> tablaempresa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaempresa> tablaempresa1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaempresa> tablaempresa2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaretenciones> tablaretenciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaretenciones> tablaretenciones1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaretenciones> tablaretenciones2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaretenciones> tablaretenciones3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tablaretenciones> tablaretenciones4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable5 { get; set; }
    }
}
