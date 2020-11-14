using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class AutorizacionTallerModel
    {

        public int id_encabezado { get; set; }
        public string id_encabezado_encryptado { get; set; }
        public List<ItemAutorizado> listaItems;

        public AutorizacionTallerModel()
        {
            listaItems = new List<ItemAutorizado>();
        }

    }


    public class ItemAutorizado
    {
        public int id_item { get; set; }
        public string nombre_item { get; set; }
        public decimal valor_item { get; set; }
        public int cantidad_item { get; set; }
        public bool autorizado { get; set; }
    }

}