using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class horarioPlantaModel
    {
        public  int asesorid { get; set; }
        public  DateTime fecha { get; set; }
        public string hora_desde { get; set; }
        public string hora_hasta { get; set; }
        public string hora_desde1 { get; set; }
        public string hora_hasta1 { get; set; }
        public string fecha2 { get; set; }

        public Nullable<System.TimeSpan> horadesde { get; set; }

        public Nullable<System.TimeSpan> horahasta { get; set; }

        public Nullable<System.TimeSpan>  horadesde1 { get; set; }
        public Nullable<System.TimeSpan> horahasta1 { get; set; }

        public bool ? checks { get; set; }
        public bool ? turno { get; set; }
        public bool ? disponible { get; set; }

    }
}