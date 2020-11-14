using System;

namespace Homer_MVC.Models
{
    public class ContratoModel
    {

        public int idcontrato { get; set; }
        public int tipocontrato { get; set; }
        public string numerocontrato { get; set; }
        public int tercero { get; set; }
        public string valorcontrato { get; set; }
        public string valordescontado { get; set; }
        public int? porceniva { get; set; }
        public System.DateTime? fechainicial { get; set; }
        public System.DateTime? fechafinal { get; set; }
        public int userid_creacion { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }

    }
}