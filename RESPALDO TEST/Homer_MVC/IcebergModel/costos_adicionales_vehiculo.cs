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
    
    public partial class costos_adicionales_vehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public costos_adicionales_vehiculo()
        {
            this.detalle_costos_adicionales_vehiculo = new HashSet<detalle_costos_adicionales_vehiculo>();
        }
    
        public int id_costo_adional_vehiculo { get; set; }
        public string nombre_costo { get; set; }
        public decimal porcentaje_costo { get; set; }
        public bool estado { get; set; }
        public bool obligatorio_opcional { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detalle_costos_adicionales_vehiculo> detalle_costos_adicionales_vehiculo { get; set; }
    }
}
