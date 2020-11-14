using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public partial class Prospectos
    {


        public int id { get; set; }
        public Nullable<int> tpdoc_id { get; set; }
        public string documento { get; set; }
        public string prinom_tercero { get; set; }
        public string segnom_tercero { get; set; }
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }
        public string razonsocial { get; set; }
        public string digitoverificacion { get; set; }
        public string email_tercero { get; set; }
        public string telf_tercero { get; set; }
        public string celular_tercero { get; set; }
        public Nullable<int> genero_tercero { get; set; }
        public int tptramite { get; set; }
        public Nullable<int> asesor_id { get; set; }
        public Nullable<int> origen_id { get; set; }
        public Nullable<int> subfuente { get; set; }
        public Nullable<int> medcomun_id { get; set; }
        public Nullable<int> sitioevento { get; set; }
        public string observaciones { get; set; }
        public bool habdtautor_correo { get; set; }
        public bool habdtautor_celular { get; set; }
        public bool habdtautor_msm { get; set; }
        public bool habdtautor_watsap { get; set; }
        public Nullable<int> numfamiliares { get; set; }
        public Nullable<int> numamigos { get; set; }
        public int bodega { get; set; }
        public int estado { get; set; }
        public Nullable<int> temperatura { get; set; }
        public Nullable<int> licencia { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<int> userid_creacion { get; set; }
        public Nullable<int> idtercero { get; set; }
        public string modelo { get; set; }
        public string modelo1 { get; set; }
        public bool visita { get; set; }
        public Nullable<int> asesores_id { get; set; }

    }
}