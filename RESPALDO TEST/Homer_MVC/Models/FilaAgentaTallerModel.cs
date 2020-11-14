using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class FilaAgentaTallerModel
    {
        public List<CeldaAgentaTallerModel> ListaCeldas;
        public List<CeldaAgentaTallerModel> ListaTitulosAgenda;

        public FilaAgentaTallerModel()
        {
            ListaCeldas = new List<CeldaAgentaTallerModel>();
            ListaTitulosAgenda = new List<CeldaAgentaTallerModel>();
        }



    }
}