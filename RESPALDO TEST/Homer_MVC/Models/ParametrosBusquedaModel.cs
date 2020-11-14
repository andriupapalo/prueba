using System.Collections.Generic;
using System.Linq;

namespace Homer_MVC.IcebergModel
{
    public class ParametrosBusquedaModel
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public List<menu_busqueda> ParametrosBusqueda(int id)
        {
            List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == id).ToList();
            return parametrosVista;
        }


        public string EnlacesBusqueda(int id)
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == id);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            return enlaces;
        }
    }
}