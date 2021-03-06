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
    
    public partial class vpedido
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public vpedido()
        {
            this.bitacoraExcepcionFactura = new HashSet<bitacoraExcepcionFactura>();
            this.crucedoc_pagos_recibidos = new HashSet<crucedoc_pagos_recibidos>();
            this.documentos_vehiculo = new HashSet<documentos_vehiculo>();
            this.encab_documento = new HashSet<encab_documento>();
            this.facturaProforma = new HashSet<facturaProforma>();
            this.icb_bahia_alistamiento = new HashSet<icb_bahia_alistamiento>();
            this.tencabezaorden = new HashSet<tencabezaorden>();
            this.v_creditos = new HashSet<v_creditos>();
            this.vpedcostos_adicionales = new HashSet<vpedcostos_adicionales>();
            this.vpedpago = new HashSet<vpedpago>();
            this.vpedrepuestos = new HashSet<vpedrepuestos>();
            this.vpedretoma = new HashSet<vpedretoma>();
            this.vpedseguimiento = new HashSet<vpedseguimiento>();
            this.vvalidacionpeddoc = new HashSet<vvalidacionpeddoc>();
        }
    
        public int id { get; set; }
        public int bodega { get; set; }
        public Nullable<int> numero { get; set; }
        public Nullable<int> idcotizacion { get; set; }
        public Nullable<int> nit { get; set; }
        public Nullable<int> nit_asegurado { get; set; }
        public Nullable<int> nit2 { get; set; }
        public Nullable<int> nit3 { get; set; }
        public Nullable<int> nit4 { get; set; }
        public Nullable<int> nit5 { get; set; }
        public bool impfactura2 { get; set; }
        public bool impfactura3 { get; set; }
        public bool impfactura4 { get; set; }
        public Nullable<int> vendedor { get; set; }
        public string modelo { get; set; }
        public System.DateTime fecha { get; set; }
        public Nullable<int> id_anio_modelo { get; set; }
        public Nullable<int> plan_venta { get; set; }
        public string planmayor { get; set; }
        public Nullable<System.DateTime> fecha_asignacion_planmayor { get; set; }
        public Nullable<int> asignado_por { get; set; }
        public Nullable<int> condicion { get; set; }
        public Nullable<int> dias_validez { get; set; }
        public Nullable<decimal> valor_unitario { get; set; }
        public Nullable<float> porcentaje_iva { get; set; }
        public Nullable<decimal> valorPoliza { get; set; }
        public Nullable<float> pordscto { get; set; }
        public Nullable<decimal> vrdescuento { get; set; }
        public Nullable<int> cantidad { get; set; }
        public Nullable<int> tipo_carroceria { get; set; }
        public Nullable<decimal> vrcarroceria { get; set; }
        public Nullable<decimal> vrtotal { get; set; }
        public bool anulado { get; set; }
        public Nullable<int> moneda { get; set; }
        public Nullable<int> id_aseguradora { get; set; }
        public string notas1 { get; set; }
        public string notas2 { get; set; }
        public bool escanje { get; set; }
        public bool eschevyplan { get; set; }
        public bool esreposicion { get; set; }
        public bool esLeasing { get; set; }
        public Nullable<int> nit_prenda { get; set; }
        public Nullable<int> flota { get; set; }
        public bool facturado { get; set; }
        public Nullable<int> numfactura { get; set; }
        public Nullable<float> porcentaje_impoconsumo { get; set; }
        public string numeroplaca { get; set; }
        public string motivo_anulacion { get; set; }
        public bool venta_gerencia { get; set; }
        public string Color_Deseado { get; set; }
        public Nullable<int> terminacionplaca { get; set; }
        public Nullable<decimal> bono { get; set; }
        public Nullable<int> idmodelo { get; set; }
        public bool nuevo { get; set; }
        public Nullable<bool> usado { get; set; }
        public Nullable<int> servicio { get; set; }
        public bool placapar { get; set; }
        public bool placaimpar { get; set; }
        public string color_opcional { get; set; }
        public Nullable<int> cargomatricula { get; set; }
        public Nullable<float> obsequioporcen { get; set; }
        public Nullable<decimal> valormatricula { get; set; }
        public string rango_placa { get; set; }
        public int marca { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public Nullable<int> codigoflota { get; set; }
        public Nullable<int> idanulacion { get; set; }
        public Nullable<int> iddepartamento { get; set; }
        public Nullable<int> idciudad { get; set; }
        public bool pazysalvo { get; set; }
        public Nullable<System.DateTime> fecpazysalvo { get; set; }
        public Nullable<int> usupazysalvo { get; set; }
        public Nullable<decimal> valorsoat { get; set; }
        public Nullable<decimal> otrosValores { get; set; }
        public Nullable<System.DateTime> fec_carroceria_envio { get; set; }
        public Nullable<System.DateTime> fec_carroceria_llegada { get; set; }
        public bool no_disponible { get; set; }
        public bool solicitado { get; set; }
        public bool para_facturar { get; set; }
        public Nullable<int> aprobado_por { get; set; }
        public Nullable<bool> tienedocumento { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bitacoraExcepcionFactura> bitacoraExcepcionFactura { get; set; }
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<crucedoc_pagos_recibidos> crucedoc_pagos_recibidos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<documentos_vehiculo> documentos_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<encab_documento> encab_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<facturaProforma> facturaProforma { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_bahia_alistamiento> icb_bahia_alistamiento { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual icb_unidad_financiera icb_unidad_financiera { get; set; }
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual modelo_vehiculo modelo_vehiculo { get; set; }
        public virtual motivoanulacion motivoanulacion { get; set; }
        public virtual nom_ciudad nom_ciudad { get; set; }
        public virtual nom_departamento nom_departamento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabezaorden> tencabezaorden { get; set; }
        public virtual tipo_carroceria tipo_carroceria1 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual users users2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<v_creditos> v_creditos { get; set; }
        public virtual vflota vflota { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedcostos_adicionales> vpedcostos_adicionales { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedpago> vpedpago { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedrepuestos> vpedrepuestos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedretoma> vpedretoma { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedseguimiento> vpedseguimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vvalidacionpeddoc> vvalidacionpeddoc { get; set; }
    }
}
