using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class encuestaPDFmodel
    {
        //encabezado
        public string nombreEncuesta { get; set; }
        public string fecha { get; set; }
        public string nombreAsesor { get; set; }
        public string nombreCliente { get; set; }
        public List<preguntasPDF> preguntas { get; set; }
    }

    public class preguntasPDF
    {

        public string pregunta { get; set; }
        public string respuesta { get; set; }

    }


}