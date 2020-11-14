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
    
    public partial class rseparacionmercancia
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public rseparacionmercancia()
        {
            this.referencias_comprometidas = new HashSet<referencias_comprometidas>();
            this.rseparacion_anticipo = new HashSet<rseparacion_anticipo>();
        }
    
        public int id { get; set; }
        public int bodega { get; set; }
        public string codigo { get; set; }
        public Nullable<int> cliente { get; set; }
        public System.DateTime fecha { get; set; }
        public int cantidad { get; set; }
        public Nullable<int> idordentaller { get; set; }
        public Nullable<int> idcita { get; set; }
        public Nullable<int> idpedido { get; set; }
        public string notas { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string placa { get; set; }
        public string color { get; set; }
        public Nullable<System.DateTime> fechaFinal { get; set; }
        public Nullable<int> separacion { get; set; }
        public Nullable<bool> solicitud { get; set; }
        public Nullable<bool> traslado { get; set; }
        public Nullable<int> bodega_traslado { get; set; }
        public Nullable<int> cantidad_recibida { get; set; }
        public Nullable<int> diasComprometidos { get; set; }
        public Nullable<bool> comprometido { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual color_vehiculo color_vehiculo { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual icb_referencia_mov icb_referencia_mov { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencias_comprometidas> referencias_comprometidas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rseparacion_anticipo> rseparacion_anticipo { get; set; }
        public virtual tencabezaorden tencabezaorden { get; set; }
        public virtual users users { get; set; }
    }
}
