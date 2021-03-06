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
    
    public partial class icb_referencia_mov
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_referencia_mov()
        {
            this.referencias_comprometidas = new HashSet<referencias_comprometidas>();
            this.icb_referencia_movdetalle = new HashSet<icb_referencia_movdetalle>();
            this.rseparacionmercancia = new HashSet<rseparacionmercancia>();
        }
    
        public int refmov_id { get; set; }
        public int tpdocid { get; set; }
        public int bodega_id { get; set; }
        public long refmov_numero { get; set; }
        public System.DateTime refmov_fecela { get; set; }
        public Nullable<int> proveedor_id { get; set; }
        public int cliente { get; set; }
        public int vendedor { get; set; }
        public int condicion { get; set; }
        public short dias_validez { get; set; }
        public decimal valor_total { get; set; }
        public string notas { get; set; }
        public Nullable<short> concepto { get; set; }
        public Nullable<int> nit_destino { get; set; }
        public decimal fletes { get; set; }
        public decimal iva_fletes { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public decimal descuento_pie { get; set; }
        public Nullable<int> idcotizacion { get; set; }
        public decimal valoriva { get; set; }
        public decimal valordescuento { get; set; }
        public Nullable<decimal> margen_utilidad { get; set; }
        public Nullable<bool> habilitar { get; set; }
        public Nullable<int> idanulacion { get; set; }
        public Nullable<System.DateTime> fecha_vencimiento { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual fpago_tercero fpago_tercero { get; set; }
        public virtual motivo_anulacion motivo_anulacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencias_comprometidas> referencias_comprometidas { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual tp_doc_registros tp_doc_registros { get; set; }
        public virtual users users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_referencia_movdetalle> icb_referencia_movdetalle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rseparacionmercancia> rseparacionmercancia { get; set; }
    }
}
