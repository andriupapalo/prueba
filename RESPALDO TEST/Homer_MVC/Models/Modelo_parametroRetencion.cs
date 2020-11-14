using System;

namespace Homer_MVC.Models
{
    public class Modelo_parametroRetencion
    {
        public int id { get; set; }
        public string concepto { get; set; }
        public int baseuvt { get; set; }
        public string basepesos { get; set; }
        public string tarifas { get; set; }
        public int id_licencia { get; set; }
        public DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public DateTime fec_actualizacion { get; set; }
        public int user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    }

}