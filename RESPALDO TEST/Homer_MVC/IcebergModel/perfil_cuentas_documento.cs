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
    
    public partial class perfil_cuentas_documento
    {
        public int id { get; set; }
        public int id_perfil { get; set; }
        public int id_nombre_parametro { get; set; }
        public int cuenta { get; set; }
        public int centro { get; set; }
    
        public virtual centro_costo centro_costo { get; set; }
        public virtual cuenta_puc cuenta_puc { get; set; }
        public virtual perfil_contable_documento perfil_contable_documento { get; set; }
    }
}
