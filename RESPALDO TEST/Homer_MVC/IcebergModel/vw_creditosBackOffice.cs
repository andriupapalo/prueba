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
    
    public partial class vw_creditosBackOffice
    {
        public int id { get; set; }
        public string poliza { get; set; }
        public int tercero_id { get; set; }
        public string nombre { get; set; }
        public int financiera_id { get; set; }
        public string financiera { get; set; }
        public Nullable<System.DateTime> fec_solicitud { get; set; }
        public Nullable<System.DateTime> fec_desembolso { get; set; }
        public string estadoc { get; set; }
        public Nullable<int> pedido { get; set; }
        public Nullable<System.DateTime> estado { get; set; }
        public Nullable<int> valorP { get; set; }
        public string planmayor { get; set; }
        public Nullable<bool> facturado { get; set; }
        public string ref_descripcion { get; set; }
    }
}
