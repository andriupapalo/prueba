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
    
    public partial class recibidotraslados
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public recibidotraslados()
        {
            this.agenda_mensajero = new HashSet<agenda_mensajero>();
            this.detalle_agendamiento = new HashSet<detalle_agendamiento>();
            this.referencias_comprometidas = new HashSet<referencias_comprometidas>();
            this.referencias_comprometidas1 = new HashSet<referencias_comprometidas>();
            this.tracking_traslado = new HashSet<tracking_traslado>();
        }
    
        public int id { get; set; }
        public int idtraslado { get; set; }
        public string refcodigo { get; set; }
        public int cantidad { get; set; }
        public bool recibido { get; set; }
        public Nullable<System.DateTime> fecharecibido { get; set; }
        public Nullable<int> userrecibido { get; set; }
        public System.DateTime fechatraslado { get; set; }
        public int usertraslado { get; set; }
        public decimal costo { get; set; }
        public string notas { get; set; }
        public int idorigen { get; set; }
        public int iddestino { get; set; }
        public string tipo { get; set; }
        public Nullable<int> cant_recibida { get; set; }
        public bool recibo_completo { get; set; }
        public bool devolucion { get; set; }
        public Nullable<int> idlinea { get; set; }
        public Nullable<int> estado_traslado { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_mensajero> agenda_mensajero { get; set; }
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual bodega_concesionario bodega_concesionario1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detalle_agendamiento> detalle_agendamiento { get; set; }
        public virtual encab_documento encab_documento { get; set; }
        public virtual Estado Estado { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual lineas_documento lineas_documento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencias_comprometidas> referencias_comprometidas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencias_comprometidas> referencias_comprometidas1 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tracking_traslado> tracking_traslado { get; set; }
    }
}
