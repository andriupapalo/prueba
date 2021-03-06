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
    
    public partial class v_creditos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public v_creditos()
        {
            this.refpersonales_cred = new HashSet<refpersonales_cred>();
            this.vdocrequeridos_credito = new HashSet<vdocrequeridos_credito>();
        }
    
        public int Id { get; set; }
        public int infocredito_id { get; set; }
        public Nullable<int> pedido { get; set; }
        public int financiera_id { get; set; }
        public Nullable<System.DateTime> fec_solicitud { get; set; }
        public Nullable<System.DateTime> fec_aprobacion { get; set; }
        public Nullable<System.DateTime> fec_negacion { get; set; }
        public Nullable<System.DateTime> fec_desembolso { get; set; }
        public Nullable<System.DateTime> fec_confirmacion { get; set; }
        public Nullable<System.DateTime> fec_envdocumentos { get; set; }
        public Nullable<System.DateTime> fec_entdocumentos { get; set; }
        public Nullable<decimal> vsolicitado { get; set; }
        public Nullable<decimal> vaprobado { get; set; }
        public string estadoc { get; set; }
        public string detalle { get; set; }
        public string num_aprobacion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public bool toma_credito { get; set; }
        public Nullable<int> plazo { get; set; }
        public Nullable<int> plan_id { get; set; }
        public Nullable<decimal> cuota_inicial { get; set; }
        public Nullable<int> asesor_id { get; set; }
        public Nullable<int> bodegaid { get; set; }
        public Nullable<int> concesionarioid { get; set; }
        public Nullable<System.DateTime> fec_desistimiento { get; set; }
        public Nullable<int> motivodesiste { get; set; }
        public bool comison { get; set; }
        public Nullable<decimal> valor_comision { get; set; }
        public Nullable<System.DateTime> fec_facturacomision { get; set; }
        public Nullable<long> numfactura { get; set; }
        public string vehiculo { get; set; }
        public string poliza { get; set; }
        public Nullable<int> idmotcomision { get; set; }
        public Nullable<int> idmotnegacion { get; set; }
    
        public virtual icb_unidad_financiera icb_unidad_financiera { get; set; }
        public virtual motivosNegacion motivosNegacion { get; set; }
        public virtual motivosNegacion motivosNegacion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<refpersonales_cred> refpersonales_cred { get; set; }
        public virtual users users { get; set; }
        public virtual vcredmotdesistio vcredmotdesistio { get; set; }
        public virtual vinfcredito vinfcredito { get; set; }
        public virtual vmotivocomision vmotivocomision { get; set; }
        public virtual vpedido vpedido { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vdocrequeridos_credito> vdocrequeridos_credito { get; set; }
    }
}
