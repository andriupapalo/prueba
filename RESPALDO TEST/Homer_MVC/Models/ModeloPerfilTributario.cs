using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class ModeloPerfilTributario
    {
        public int perfilTributario_id { get; set; }
        public int bodega { get; set; }
        public int sw { get; set; }
        public int tipo_regimenid { get; set; }
        public string retfuente { get; set; }
        public string retiva { get; set; }
        public string retica { get; set; }
        public string retcree { get; set; }
        public string ret1 { get; set; }
        public string ret2 { get; set; }
        public string ret3 { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string autorretencion { get; set; }
        public Nullable<decimal> pretfuente { get; set; }
        public Nullable<decimal> pretiva { get; set; }
        public Nullable<decimal> pretica { get; set; }
        public Nullable<decimal> baseretfuente { get; set; }
        public Nullable<decimal> baseretiva { get; set; }
        public Nullable<decimal> baseretica { get; set; }

        public List<Conceptos> conceptos { get; set; }
    }

    public class Conceptos
    {
        public int id { get; set; }
        public int id_bodega { get; set; }
        public int id_sw { get; set; }
        public int id_idregimen { get; set; }
        public int id_retencion { get; set; }

        public int id_concepto { get; set; }

        public Nullable<decimal> porcentaje { get; set; }

        public Nullable<decimal> baserete { get; set; }





    }
}