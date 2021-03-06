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
    
    public partial class bodega_concesionario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bodega_concesionario()
        {
            this.aencab_auditoriainv = new HashSet<aencab_auditoriainv>();
            this.agenda_asesor = new HashSet<agenda_asesor>();
            this.agenda_demos = new HashSet<agenda_demos>();
            this.agenda_mensajero = new HashSet<agenda_mensajero>();
            this.agenda_mensajero1 = new HashSet<agenda_mensajero>();
            this.area_bodega = new HashSet<area_bodega>();
            this.autorizaciones = new HashSet<autorizaciones>();
            this.bitacoraExcepcionFactura = new HashSet<bitacoraExcepcionFactura>();
            this.centro_costo = new HashSet<centro_costo>();
            this.icb_caja = new HashSet<icb_caja>();
            this.icb_consecutivo_ot = new HashSet<icb_consecutivo_ot>();
            this.icb_cuadre_caja = new HashSet<icb_cuadre_caja>();
            this.indicadores_gen_Bodega = new HashSet<indicadores_gen_Bodega>();
            this.perfilbodegastipocompra = new HashSet<perfilbodegastipocompra>();
            this.Solicitud_traslado = new HashSet<Solicitud_traslado>();
            this.Solicitud_traslado1 = new HashSet<Solicitud_traslado>();
            this.tempSolicitud = new HashSet<tempSolicitud>();
            this.tempSolicitud1 = new HashSet<tempSolicitud>();
            this.tordentot = new HashSet<tordentot>();
            this.bodega_usuario_visualizacion = new HashSet<bodega_usuario_visualizacion>();
            this.documentos_bodega = new HashSet<documentos_bodega>();
            this.encab_documento = new HashSet<encab_documento>();
            this.grupoconsecutivos = new HashSet<grupoconsecutivos>();
            this.icb_doc_consecutivos = new HashSet<icb_doc_consecutivos>();
            this.icb_referencia_mov = new HashSet<icb_referencia_mov>();
            this.icb_terceros = new HashSet<icb_terceros>();
            this.icb_vehiculo = new HashSet<icb_vehiculo>();
            this.icb_vehiculo_eventos = new HashSet<icb_vehiculo_eventos>();
            this.mesactualbodega = new HashSet<mesactualbodega>();
            this.metas_asesor = new HashSet<metas_asesor>();
            this.pedido_tliberacion = new HashSet<pedido_tliberacion>();
            this.perfil_contable_bodega = new HashSet<perfil_contable_bodega>();
            this.planfinancierobodega = new HashSet<planfinancierobodega>();
            this.prospectos = new HashSet<prospectos>();
            this.recibidotraslados = new HashSet<recibidotraslados>();
            this.recibidotraslados1 = new HashSet<recibidotraslados>();
            this.referencias_inven = new HashSet<referencias_inven>();
            this.retencionesbodega = new HashSet<retencionesbodega>();
            this.rordencompra = new HashSet<rordencompra>();
            this.rseparacionmercancia = new HashSet<rseparacionmercancia>();
            this.sesion_logasesor = new HashSet<sesion_logasesor>();
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.tencabezaorden_exh = new HashSet<tencabezaorden_exh>();
            this.terceros_bod_ica = new HashSet<terceros_bod_ica>();
            this.tparametrocontable = new HashSet<tparametrocontable>();
            this.ttarifastaller = new HashSet<ttarifastaller>();
            this.ubicacion_bodega = new HashSet<ubicacion_bodega>();
            this.ubicacion_repuesto = new HashSet<ubicacion_repuesto>();
            this.valortramitebodega = new HashSet<valortramitebodega>();
            this.vpedido = new HashSet<vpedido>();
        }
    
        public int id { get; set; }
        public string bodccs_cod { get; set; }
        public string bodccs_nombre { get; set; }
        public string bodccs_direccion { get; set; }
        public int concesionarioid { get; set; }
        public Nullable<int> bodccsid_licencia { get; set; }
        public System.DateTime bodccsfec_creacion { get; set; }
        public int bodccsuserid_creacion { get; set; }
        public Nullable<System.DateTime> bodccsfec_actualizacion { get; set; }
        public Nullable<int> bodccsuserid_actualizacion { get; set; }
        public bool bodccs_estado { get; set; }
        public string bodccsrazoninactivo { get; set; }
        public Nullable<int> bodccscentro_id { get; set; }
        public bool es_puntoventa { get; set; }
        public bool es_taller { get; set; }
        public bool es_repuestos { get; set; }
        public bool es_vehiculos { get; set; }
        public Nullable<int> pais_id { get; set; }
        public Nullable<int> ciudad_id { get; set; }
        public Nullable<int> departamento_id { get; set; }
        public string codigobac { get; set; }
        public Nullable<System.TimeSpan> hora_inicial { get; set; }
        public Nullable<System.TimeSpan> hora_final { get; set; }
        public Nullable<int> lapso_tiempo { get; set; }
        public Nullable<int> porcentaje_anticipo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<aencab_auditoriainv> aencab_auditoriainv { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_asesor> agenda_asesor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_demos> agenda_demos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_mensajero> agenda_mensajero { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_mensajero> agenda_mensajero1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<area_bodega> area_bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<autorizaciones> autorizaciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bitacoraExcepcionFactura> bitacoraExcepcionFactura { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<centro_costo> centro_costo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_caja> icb_caja { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_consecutivo_ot> icb_consecutivo_ot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_cuadre_caja> icb_cuadre_caja { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<indicadores_gen_Bodega> indicadores_gen_Bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<perfilbodegastipocompra> perfilbodegastipocompra { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Solicitud_traslado> Solicitud_traslado { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Solicitud_traslado> Solicitud_traslado1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tempSolicitud> tempSolicitud { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tempSolicitud> tempSolicitud1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tordentot> tordentot { get; set; }
        public virtual nom_ciudad nom_ciudad { get; set; }
        public virtual nom_departamento nom_departamento { get; set; }
        public virtual nom_pais nom_pais { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bodega_usuario_visualizacion> bodega_usuario_visualizacion { get; set; }
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
        public virtual ICollection<icb_terceros> icb_terceros { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo> icb_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo_eventos> icb_vehiculo_eventos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mesactualbodega> mesactualbodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<metas_asesor> metas_asesor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pedido_tliberacion> pedido_tliberacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<perfil_contable_bodega> perfil_contable_bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<planfinancierobodega> planfinancierobodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<prospectos> prospectos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<recibidotraslados> recibidotraslados { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<recibidotraslados> recibidotraslados1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencias_inven> referencias_inven { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<retencionesbodega> retencionesbodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rordencompra> rordencompra { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rseparacionmercancia> rseparacionmercancia { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sesion_logasesor> sesion_logasesor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden_exh> tencabezaorden_exh { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<terceros_bod_ica> terceros_bod_ica { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tparametrocontable> tparametrocontable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ttarifastaller> ttarifastaller { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicacion_bodega> ubicacion_bodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicacion_repuesto> ubicacion_repuesto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<valortramitebodega> valortramitebodega { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedido> vpedido { get; set; }
    }
}
