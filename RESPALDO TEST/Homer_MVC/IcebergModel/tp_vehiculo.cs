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
    
    public partial class tp_vehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tp_vehiculo()
        {
            this.pedido_tliberacion = new HashSet<pedido_tliberacion>();
        }
    
        public int id { get; set; }
        public string nombre { get; set; }
        public Nullable<int> user_creacion { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<int> user_actualizacion { get; set; }
        public Nullable<System.DateTime> fec_actualziacion { get; set; }
        public Nullable<int> licencia_id { get; set; }
        public Nullable<int> bodega_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pedido_tliberacion> pedido_tliberacion { get; set; }
    }
}