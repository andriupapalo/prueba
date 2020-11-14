using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class PdfOrdenTaller
    {
        
        public int? id { get; set; }
        public string idtipodoc { get; set; }
        public string codigoentrada { get; set; }
        public string bodega { get; set; }
        public string fecha_generacion { get; set; }
        public string razon_inactivo { get; set; }
        public string asesor { get; set; }
        public string tecnico { get; set; }
        public string razoningreso { get; set; }
        public string aseguradora { get; set; }
        public string poliza { get; set; }
        public string siniestro { get; set; }
        public string deducible { get; set; }
        public string minimo { get; set; }
        public string fecha_soat { get; set; }
        public string numero_soat { get; set; }
        public string garantia_falla { get; set; }
        public string garantia_causa { get; set; }
        public string garantia_solucion { get; set; }
        public string fecha_entrada_vh { get; set; }
        public string fecha_prometida { get; set; }
        public string fecha_ingreso { get; set; }
        public string otrocontacto { get; set; }
        public string telotrocontacto { get; set; }
        public string contacto { get; set; }
        public string numcontacto { get; set; }
        public string txtDocumentoCliente { get; set; }
        public string nombrecliente { get; set; }
        public string telefonocliente { get; set; }
        public string celularcliente { get; set; }
        public string correocliente { get; set; }
        public string ciudadcliente { get; set; }


        public string placa { get; set; }
        public string serie { get; set; }
        public string modelo { get; set; }
        public int? anio_modelo { get; set; }
        public string numero_motor { get; set; }
        public string color { get; set; }
        public string fecha_venta { get; set; }
        public string fecha_fin_garantia { get; set; }
        public string kilometraje { get; set; }
        public string kilometraje_actual { get; set; }

        public List<sintomas> sintomas { get; set; }

    }

    public class sintomas
    {
        public int id_solicitud { get; set; }
        public string descripcion_solicitud { get; set; }
        public string respuesta_taller { get; set; }
    }

}