using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class PDFRetiroInterno_icbrefmov
        {

        public string CodigoRetiro { get; set; }
        public string Fechadocumento { get; set; }
        public string Bodega { get; set; }
        public string Aseguradoa { get; set; }
        public string Documentocliente { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Ciudad { get; set; }
        public string Placa { get; set; }
        public string Vehiculo { get; set; }
        public string Serie { get; set; }
        public string Modelo { get; set; }
        public string Kilometraje { get; set; }
        public string OrdenT { get; set; }
        public string Facturadopor { get; set; }
        public string Asesor { get; set; }
        public List<lineas_documento> Repuestosr { get; set; }
        }
    }