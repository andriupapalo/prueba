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
    
    public partial class ttecnicos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ttecnicos()
        {
            this.icb_vehiculo_eventos = new HashSet<icb_vehiculo_eventos>();
            this.tcitastaller = new HashSet<tcitastaller>();
            this.tdetallemanoobraot = new HashSet<tdetallemanoobraot>();
            this.tencabinspeccion = new HashSet<tencabinspeccion>();
        }
    
        public int id { get; set; }
        public int idusuario { get; set; }
        public int tipo_tecnico { get; set; }
        public bool contratista { get; set; }
        public Nullable<decimal> valorhora { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<System.TimeSpan> iniciodescanso { get; set; }
        public Nullable<System.TimeSpan> findescanso { get; set; }
        public Nullable<decimal> porcenhora { get; set; }
        public bool otros_casos { get; set; }
        public string claveSeguridad { get; set; }
        public string codigo_operario { get; set; }
        public bool condicion_adicional { get; set; }
        public Nullable<decimal> valor_adicional { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_vehiculo_eventos> icb_vehiculo_eventos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tcitastaller> tcitastaller { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tdetallemanoobraot> tdetallemanoobraot { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tencabinspeccion> tencabinspeccion { get; set; }
        public virtual ttipotecnico ttipotecnico { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual users users2 { get; set; }
    }
}