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
    
    public partial class rsolicitudesrepuestos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public rsolicitudesrepuestos()
        {
            this.rdetallesolicitud = new HashSet<rdetallesolicitud>();
            this.robservacion_solicitud = new HashSet<robservacion_solicitud>();
        }
    
        public int id { get; set; }
        public int bodega { get; set; }
        public string ref_codigo { get; set; }
        public System.DateTime fecha { get; set; }
        public Nullable<int> cliente { get; set; }
        public Nullable<int> cantidad { get; set; }
        public int usuario { get; set; }
        public string Detalle { get; set; }
        public Nullable<int> Pedido { get; set; }
        public int tiposolicitud { get; set; }
        public Nullable<int> id_ot { get; set; }
        public Nullable<int> separacion_consecutivo { get; set; }
        public Nullable<int> estado_solicitud { get; set; }
        public Nullable<int> tipo_compra { get; set; }
        public string planm_vehiculo { get; set; }
        public string modelo_vehiculo { get; set; }
        public Nullable<int> clasificacion_solicitud { get; set; }
        public Nullable<bool> habilitado { get; set; }
        public string motivo { get; set; }
    
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual rclasificacion_solicitud rclasificacion_solicitud { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rdetallesolicitud> rdetallesolicitud { get; set; }
        public virtual restado_solicitud_Repuestos restado_solicitud_Repuestos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<robservacion_solicitud> robservacion_solicitud { get; set; }
        public virtual rtipocompra rtipocompra { get; set; }
    }
}