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
    
    public partial class vwDatosMensajeria
    {
        public long numero { get; set; }
        public string refcodigo { get; set; }
        public string ref_descripcion { get; set; }
        public System.DateTime desde { get; set; }
        public System.DateTime hasta { get; set; }
        public int id { get; set; }
        public string Tipo { get; set; }
        public string tipo_prioridad { get; set; }
        public string tipo_transporte { get; set; }
        public string prinom_tercero { get; set; }
        public string apellido_tercero { get; set; }
        public string descripcion { get; set; }
        public Nullable<int> bodega_origen { get; set; }
        public Nullable<int> bodega_destino { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> idEncabezado { get; set; }
        public string bodrpto_nombre { get; set; }
        public int userid_creacion { get; set; }
        public string user_nombre { get; set; }
        public string user_apellido { get; set; }
    }
}