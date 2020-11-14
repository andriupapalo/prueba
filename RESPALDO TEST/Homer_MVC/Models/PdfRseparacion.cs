using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class PdfRseparacion
        {

        public string CodigorRseparacion{ get; set; }
        public string Fechadocumento { get; set; }
        public string Bodega { get; set; }
        public string Aseguradoa { get; set; }
        public string Documentocliente { get; set; }
        public string Cliente { get; set; }
        public string Direccion { get; set; }
        public string Direccionempresa { get; set; }
        public string Telefono { get; set; }
        public string Telefonoempresa { get; set; }
        public string Ciudad { get; set; }
        public string Placa { get; set; }
        public string Vehiculo { get; set; }   
        public string Asesor { get; set; }
        public string RecCcaja { get; set; }
        public List<rseparacionmercancia> Repuestos { get; set; }



        }
    }