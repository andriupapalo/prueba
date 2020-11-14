using Homer_MVC.IcebergModel;
using System;
using System.Linq;

namespace Homer_MVC.Models
{
    public class ConsecutivosGestion
    {

        private readonly Iceberg_Context context = new Iceberg_Context();

        public icb_doc_consecutivos BuscarConsecutivo(tp_doc_registros tp_doc, int id_tipo_documento, int id_bodega)
        {
            int anioActual = DateTime.Now.Year;
            int mesActual = DateTime.Now.Month;

            if (tp_doc.consecano)
            {
                return context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == id_tipo_documento && x.doccons_bodega == id_bodega && x.doccons_ano == anioActual);
            }
            else if (tp_doc.consecmes)
            {
                return context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == id_tipo_documento && x.doccons_bodega == id_bodega && x.doccons_ano == anioActual && x.doccons_mes == mesActual);
            }
            else
            {
                return context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == id_tipo_documento && x.doccons_bodega == id_bodega);
            }
        }

    }
}