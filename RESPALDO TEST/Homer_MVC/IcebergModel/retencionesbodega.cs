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
    
    public partial class retencionesbodega
    {
        public int id { get; set; }
        public int idretencion { get; set; }
        public int idbodega { get; set; }
        public int ctaiva { get; set; }
        public int ctareteiva { get; set; }
        public int ctaretencion { get; set; }
        public int ctareteica { get; set; }
        public int cuentaxpagar { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual cuenta_puc cuenta_puc { get; set; }
        public virtual cuenta_puc cuenta_puc1 { get; set; }
        public virtual cuenta_puc cuenta_puc2 { get; set; }
        public virtual cuenta_puc cuenta_puc3 { get; set; }
        public virtual cuenta_puc cuenta_puc4 { get; set; }
        public virtual tablaretenciones tablaretenciones { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}