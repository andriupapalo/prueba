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
    
    public partial class icb_tsistemas_operaciones
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_tsistemas_operaciones()
        {
            this.icb_tinspeccionsistemasvh = new HashSet<icb_tinspeccionsistemasvh>();
        }
    
        public int tsope_id { get; set; }
        public Nullable<int> tsis_id { get; set; }
        public string ttemp_codigo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_tinspeccionsistemasvh> icb_tinspeccionsistemasvh { get; set; }
        public virtual icb_tsistemas icb_tsistemas { get; set; }
        public virtual ttempario ttempario { get; set; }
    }
}