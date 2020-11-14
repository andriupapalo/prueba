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
    
    public partial class modelo_vehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public modelo_vehiculo()
        {
            this.anio_modelo = new HashSet<anio_modelo>();
            this.detallePedido_GM = new HashSet<detallePedido_GM>();
            this.estilo_vehiculo = new HashSet<estilo_vehiculo>();
            this.icb_hist_prospecto = new HashSet<icb_hist_prospecto>();
            this.icb_plan_pago = new HashSet<icb_plan_pago>();
            this.icb_vehiculo = new HashSet<icb_vehiculo>();
            this.pcotizaciondigital = new HashSet<pcotizaciondigital>();
            this.prospectos = new HashSet<prospectos>();
            this.prospectos1 = new HashSet<prospectos>();
            this.tcitastaller = new HashSet<tcitastaller>();
            this.vaccesoriomodelo = new HashSet<vaccesoriomodelo>();
            this.vcotdetallevehiculo = new HashSet<vcotdetallevehiculo>();
            this.vcotrepuestos = new HashSet<vcotrepuestos>();
            this.vdescuentoscondicionados = new HashSet<vdescuentoscondicionados>();
            this.vpedido = new HashSet<vpedido>();
        }
    
        public string modvh_codigo { get; set; }
        public Nullable<int> modvhid_licencia { get; set; }
        public System.DateTime modvhfec_creacion { get; set; }
        public int modvhuserid_creacion { get; set; }
        public Nullable<System.DateTime> modvhfec_actualizacion { get; set; }
        public Nullable<int> modvhuserid_actualizacion { get; set; }
        public string modvh_nombre { get; set; }
        public bool modvh_estado { get; set; }
        public string modvhrazoninactivo { get; set; }
        public Nullable<int> mar_vh_id { get; set; }
        public Nullable<int> seg_vh_id { get; set; }
        public Nullable<decimal> capacidad { get; set; }
        public Nullable<int> cilindraje { get; set; }
        public Nullable<int> combustible { get; set; }
        public Nullable<int> grupo { get; set; }
        public Nullable<int> tipo { get; set; }
        public Nullable<int> clase { get; set; }
        public Nullable<int> perfil { get; set; }
        public Nullable<int> unidadcarga { get; set; }
        public Nullable<int> tipocaja { get; set; }
        public Nullable<int> clasificacion { get; set; }
        public Nullable<int> diaslibrescaplan { get; set; }
        public Nullable<int> concesionarioid { get; set; }
        public Nullable<int> modelogkit { get; set; }
        public Nullable<int> diaslibresgmac { get; set; }
        public Nullable<int> idequipamiento { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<anio_modelo> anio_modelo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detallePedido_GM> detallePedido_GM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<estilo_vehiculo> estilo_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_hist_prospecto> icb_hist_prospecto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_plan_pago> icb_plan_pago { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo> icb_vehiculo { get; set; }
        public virtual marca_vehiculo marca_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pcotizaciondigital> pcotizaciondigital { get; set; }
        public virtual segmento_vehiculo segmento_vehiculo { get; set; }
        public virtual tpmotor_vehiculo tpmotor_vehiculo { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual vequipamiento vequipamiento { get; set; }
        public virtual vmodelog vmodelog { get; set; }
        public virtual vtipo vtipo { get; set; }
        public virtual vunidadcarga vunidadcarga { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<prospectos> prospectos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<prospectos> prospectos1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitastaller> tcitastaller { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vaccesoriomodelo> vaccesoriomodelo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vcotdetallevehiculo> vcotdetallevehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vcotrepuestos> vcotrepuestos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vdescuentoscondicionados> vdescuentoscondicionados { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vpedido> vpedido { get; set; }
    }
}
