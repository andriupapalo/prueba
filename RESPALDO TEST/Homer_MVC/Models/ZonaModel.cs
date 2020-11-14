using System.Collections.Generic;

namespace Homer_MVC.IcebergModel
{
    public class ZonaModel
    {
        public ZonaModel()
        {
            lista = new List<ZonaModel>();
            piezas = new List<ZonaModel>();
            convenciones = new List<ZonaModel>();
        }

        public int id { get; set; }
        public string nombre { get; set; }
        public List<ZonaModel> lista { get; set; }
        public List<ZonaModel> piezas { get; set; }
        public List<ZonaModel> convenciones { get; set; }
    }
}