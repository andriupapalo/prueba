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
    
    public partial class medios_gen
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public medios_gen()
        {
            this.medios_movtos = new HashSet<medios_movtos>();
        }
    
        public int id { get; set; }
        public int ano { get; set; }
        public int formato { get; set; }
        public int concepto { get; set; }
        public string val1 { get; set; }
        public string val2 { get; set; }
        public string val3 { get; set; }
        public string val4 { get; set; }
        public string val5 { get; set; }
        public string val6 { get; set; }
        public string val7 { get; set; }
        public string val8 { get; set; }
        public string val9 { get; set; }
        public string val10 { get; set; }
        public string val11 { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<medios_movtos> medios_movtos { get; set; }
    }
}
