using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class CheckIngresoModel
    {
        /// <summary>
        /// //
        /// </summary>

        public bool recepcion { get; set; }
        public bool entrega { get; set; }
        public string cedula { get; set; }
        public string cliente { get; set; }
        public string vehiculo { get; set; }
        public string color { get; set; }
        public int modelo { get; set; }
        public DateTime fecha { get; set; }
        public string serie { get; set; }
        public string placa { get; set; }
        public string peritaje { get; set; }
        public int km { get; set; }
        public string codigo { get; set; }
        public string version { get; set; }
        public string observaciones { get; set; }
        public int numeroConsecutivo { get; set; }

        public List<PreguntasIngresoModel> preguntas1 { get; set; }
        public List<PreguntasIngresoModel> preguntas2 { get; set; }

        public CheckIngresoModel()
        {
            preguntas1 = new List<PreguntasIngresoModel>();
            preguntas2 = new List<PreguntasIngresoModel>();
        }

    }


}