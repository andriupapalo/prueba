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
    
    public partial class vw_prospecto
    {
        public int id { get; set; }
        public string prinom_tercero { get; set; }
        public string segnom_tercero { get; set; }
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }
        public string razonsocial { get; set; }
        public string digitoverificacion { get; set; }
        public string documento { get; set; }
        public string telefono { get; set; }
        public string celular { get; set; }
        public string correo { get; set; }
        public string direccion { get; set; }
        public Nullable<int> asesor_id { get; set; }
        public string asesor { get; set; }
        public string tramite { get; set; }
        public int tporigen_id { get; set; }
        public string tporigen_nombre { get; set; }
        public int id_bodega { get; set; }
        public string bodccs_nombre { get; set; }
        public Nullable<System.DateTime> fechaInicial { get; set; }
        public int tptrapros_id { get; set; }
        public Nullable<int> userid_creacion { get; set; }
        public Nullable<int> rol_id { get; set; }
        public string rol_nombre { get; set; }
        public Nullable<int> idtercero { get; set; }
    }
}