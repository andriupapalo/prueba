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
    
    public partial class area_bodega
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public area_bodega()
        {
            this.estanterias = new HashSet<estanterias>();
            this.icb_caja = new HashSet<icb_caja>();
            this.ubicacion_repuesto = new HashSet<ubicacion_repuesto>();
        }
    
        public int areabod_id { get; set; }
        public Nullable<int> areabodid_licencia { get; set; }
        public System.DateTime areabodfec_creacion { get; set; }
        public int areaboduserid_creacion { get; set; }
        public Nullable<System.DateTime> areabodfec_actualizacion { get; set; }
        public Nullable<int> areaboduserid_actualizacion { get; set; }
        public string areabod_nombre { get; set; }
        public bool areabod_estado { get; set; }
        public string areabodrazoninactivo { get; set; }
        public int id_bodega { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<estanterias> estanterias { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_caja> icb_caja { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ubicacion_repuesto> ubicacion_repuesto { get; set; }
    }
}