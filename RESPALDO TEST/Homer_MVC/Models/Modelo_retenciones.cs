using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class Modelo_retenciones
    {
        public int id { get; set; }
        public int concepto { get; set; }
        //public int baseuvt { get; set; }
        //public string basepesos { get; set; }
        //public string tarifas { get; set; }
        public int ctaimpuesto { get; set; }
        public int ctareteiva { get; set; }
        public int ctaretencion { get; set; }
        public int ctaica { get; set; }
        public int ctaxpagar { get; set; }
        public int id_licencia { get; set; }
        public DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public DateTime fec_actualizacion { get; set; }
        public int user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public List<ListaPerfiles001> ListaPerfiles001 { get; set; }
        public List<ListaBodegas001> ListaBodegas001 { get; set; }

    }
    public class ListaPerfiles001
    {
        public int id { get; set; }
        public string nomPerfil { get; set; }
    }
    public class ListaBodegas001
    {
        public int id { get; set; }
        public int idretencion { get; set; }
        public int idbodega { get; set; }
        public string nomBodega { get; set; }
    }
}