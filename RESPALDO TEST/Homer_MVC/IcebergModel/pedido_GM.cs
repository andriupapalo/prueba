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
    
    public partial class pedido_GM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public pedido_GM()
        {
            this.detallePedido_GM = new HashSet<detallePedido_GM>();
        }
    
        public long pedido_codigo { get; set; }
        public System.DateTime pedido_fecha { get; set; }
        public int pedido_usuario_id { get; set; }
        public Nullable<bool> pedido_activo { get; set; }
        public string pedido_razonInactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<detallePedido_GM> detallePedido_GM { get; set; }
    }
}
