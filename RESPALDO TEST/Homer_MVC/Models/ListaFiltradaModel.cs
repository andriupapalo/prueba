using System.Collections.Generic;
using System.Web.Mvc;

namespace Homer_MVC.Models
{
    public class ListaFiltradaModel
    {

        public string NombreAMostrar { get; set; }
        public string NombreCampo { get; set; }
        public int multiple { get; set; }
        public List<SelectListItem> items;

        public ListaFiltradaModel()
        {
            items = new List<SelectListItem>();
        }

    }
}