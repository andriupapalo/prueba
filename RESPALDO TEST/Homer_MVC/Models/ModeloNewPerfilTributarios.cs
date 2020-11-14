using System;

namespace Homer_MVC.Models
{
    public class ModeloNewPerfilTributarios
    {
        public int id { get; set; }
        public int id_Bodega { get; set; }
        public int id_SW { get; set; }
        public int id_RegimenTributario { get; set; }
        public int id_Retencion { get; set; }
        public int id_Concepto { get; set; }
        public Nullable<decimal> Pordentaje { get; set; }
        public Nullable<decimal> Base { get; set; }

        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }


    }
}