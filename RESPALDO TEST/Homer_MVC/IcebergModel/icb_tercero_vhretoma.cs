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
    
    public partial class icb_tercero_vhretoma
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_tercero_vhretoma()
        {
            this.icb_solicitud_peritaje = new HashSet<icb_solicitud_peritaje>();
        }
    
        public int veh_id { get; set; }
        public int marca_id { get; set; }
        public string modelo_codigo { get; set; }
        public Nullable<int> tp_vehiculo { get; set; }
        public string placa { get; set; }
        public Nullable<int> servicio { get; set; }
        public string color { get; set; }
        public Nullable<int> cilindraje { get; set; }
        public Nullable<int> tp_caja { get; set; }
        public string serie { get; set; }
        public string numero_motor { get; set; }
        public Nullable<int> tp_motor { get; set; }
        public string equipamiento { get; set; }
        public Nullable<int> estilo { get; set; }
        public int tercero_id { get; set; }
        public int anio { get; set; }
        public Nullable<int> ciudad_placa { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_solicitud_peritaje> icb_solicitud_peritaje { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual nom_ciudad nom_ciudad { get; set; }
    }
}