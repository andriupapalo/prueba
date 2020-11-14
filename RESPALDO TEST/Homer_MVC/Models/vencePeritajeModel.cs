using System;

namespace Homer_MVC.Models
{
    public class vencePeritajeModel
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public string valorTiempo { get; set; }
        public string valorKilometraje { get; set; }
        public bool estado { get; set; }
        public string razonInactivo { get; set; }
        public int userid_creacion { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<int> userid_actualizacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
    }
}