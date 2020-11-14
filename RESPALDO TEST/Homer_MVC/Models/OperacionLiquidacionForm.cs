using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class OperacionLiquidacionForm
    {
        [Required]
        public int? idorden { get; set; }
        [Required]
        public int? id { get; set; }
        [Required]
        public string codigoentrada { get; set; }
        [Required]
        public int? razon_ingreso { get; set; }
        [Required]
        public int? razon_ingreso2 { get; set; }
        [Required]
        public int? tipo_orden { get; set; }
        [Required]
        public int? bodega { get; set; }
        //campos opcionales de orden
        public string notas { get; set; }
        public int? numerocita { get; set; }
        public string bahia { get; set; }

        public int? aseguradora { get; set; }


        public string garantia_falla { get; set; }

        public string garantia_causa { get; set; }
        public string garantia_solucion { get; set; }

        public string poliza { get; set; }

        public string siniestro { get; set; }

        public string minimo { get; set; }
        public string deducible { get; set; }

        public string fecha_soat { get; set; }
        public string numero_soat { get; set; }
        //
        [Required]
        public int? asesor { get; set; }
        [Required]
        public int? tecnico { get; set; }
        [Required]
        public string fecha_estimada_entrega { get; set; }
        //segmento de datos del cliente
        [Required]
        public string nombre_cliente { get; set; }
        public int? id_cliente { get; set; }
        [Required]
        public string direccion_cliente { get; set; }
        [Required]
        public string telefono_cliente { get; set; }
        [Required]
        public string correo_cliente { get; set; }
        [Required]
        public string txtDocumentoCliente { get; set; }
        //[Required]
        //public string tipo_cliente { get; set; }

        //segmento de datos del vehículo
        [Required]
        public string plan_mayor { get; set; }
        [Required]
        public string placa { get; set; }
        [Required]
        public string modelo { get; set; }
        [Required]
        public string anio { get; set; }
        [Required]
        public string color { get; set; }
        [Required]
        public string kilometraje { get; set; }
        public int? id_plan_mantenimiento { get; set; }


    }
}