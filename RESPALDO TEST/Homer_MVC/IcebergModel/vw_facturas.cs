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
    
    public partial class vw_facturas
    {
        public long numero { get; set; }
        public string prefijo { get; set; }
        public string tpdoc_nombre { get; set; }
        public System.DateTime fecha { get; set; }
        public string ref_descripcion { get; set; }
        public string prinom_tercero { get; set; }
        public string segnom_tercero { get; set; }
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }
        public string razon_social { get; set; }
        public string telf_tercero { get; set; }
        public string celular_tercero { get; set; }
        public string email_tercero { get; set; }
        public string financiera_nombre { get; set; }
        public string fpago_nombre { get; set; }
        public Nullable<System.DateTime> vencimiento { get; set; }
        public decimal valor_total { get; set; }
        public Nullable<int> asesor_id { get; set; }
        public string asesor { get; set; }
        public int id_bodega { get; set; }
        public string bodccs_nombre { get; set; }
        public string documento { get; set; }
        public Nullable<bool> nuevo { get; set; }
        public Nullable<bool> usado { get; set; }
        public string tipo { get; set; }
    }
}