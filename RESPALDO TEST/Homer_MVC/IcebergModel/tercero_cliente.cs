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
    
    public partial class tercero_cliente
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tercero_cliente()
        {
            this.contratoscomerciales = new HashSet<contratoscomerciales>();
        }
    
        public int cltercero_id { get; set; }
        public Nullable<int> tercliid_licencia { get; set; }
        public Nullable<System.DateTime> terclifec_creacion { get; set; }
        public Nullable<int> tercliuserid_creacion { get; set; }
        public Nullable<System.DateTime> terclifec_actualizacion { get; set; }
        public Nullable<int> tercliuserid_actualizacion { get; set; }
        public Nullable<int> numhijos_tercero { get; set; }
        public string edades_hijos { get; set; }
        public bool tercli_estado { get; set; }
        public int tercero_id { get; set; }
        public int tpocupacion_id { get; set; }
        public int tphobby_id { get; set; }
        public int tpdpte_id { get; set; }
        public int edocivil_id { get; set; }
        public string razoninactivo { get; set; }
        public bool exentoiva { get; set; }
        public Nullable<decimal> cupocredito { get; set; }
        public Nullable<int> dia_nofacturad { get; set; }
        public Nullable<int> dia_nofacturah { get; set; }
        public float dscto_rep { get; set; }
        public float dscto_mo { get; set; }
        public Nullable<int> tipo_cliente { get; set; }
        public Nullable<int> cod_pago_id { get; set; }
        public Nullable<int> tpregimen_id { get; set; }
        public string telefono { get; set; }
        public string pagina_web { get; set; }
        public Nullable<int> base_retencion { get; set; }
        public bool retencion { get; set; }
        public bool contribucion { get; set; }
        public string lprecios_repuestos { get; set; }
        public Nullable<int> lprecios_vehiculos { get; set; }
        public bool bloqueado { get; set; }
        public string motivo_bloqueado { get; set; }
        public int tiempo_para_bloqueo { get; set; }
        public Nullable<int> actividadEconomica_id { get; set; }
        public int idsegmentacion { get; set; }
        public Nullable<System.DateTime> fec_cupo_limite { get; set; }
    
        public virtual acteco_tercero acteco_tercero { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<contratoscomerciales> contratoscomerciales { get; set; }
        public virtual fpago_tercero fpago_tercero { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual segmentacion segmentacion { get; set; }
        public virtual tpregimen_tercero tpregimen_tercero { get; set; }
    }
}
