using Homer_MVC.IcebergModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Homer_MVC.Models
{
    //[Bind(Exclude = "ttemmpario")]
    public class EncabezadoOTModel
    {
        //modi 
        public int? id { get; set; }
        [Display(Name = "Tipo Documento ")]
        [Required]
        public int? idtipodoc { get; set; }
        [Required]
        public string codigoentrada { get; set; }
        [Required]
        public int? bodega { get; set; }
        [Required]
        public string modelo { get; set; }
        [Required]
        public int? añovh { get; set; }
        [Required]
        public int? numero { get; set; }
        [Required]
        public int? tercero { get; set; }
        [Required]
        public string placa { get; set; }
        [Required]
        public string asesor { get; set; }
        [Required]
        public int? tecnico { get; set; }
        [Required]
        public string entrega { get; set; }
        [VerificarRazonIngreso]
        public Nullable<int> aseguradora { get; set; }
        [VerificarRazonIngreso]
        public string poliza { get; set; }
        [VerificarRazonIngreso]
        public string siniestro { get; set; }
        [VerificarRazonIngreso]
        public string deducible { get; set; }
        [VerificarRazonIngreso]
        public string minimo { get; set; }
        public string notas { get; set; }
        [Required]
        public long kilometraje { get; set; }
        [Required]
        public long kilometraje_nuevo { get; set; }
        [Required]
        public int razoningreso { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int? tipooperacion { get; set; }
        public bool? domicilo { get; set; }
        public int? idcita { get; set; }

        [Required]
        public string txtDocumentoCliente { get; set; }
        [Required]
        public int? estadoorden { get; set; }
        public int? centrocosto { get; set; }
        public int? razon_secundaria { get; set; }
        [ValidarGarantia]
        public string garantia_falla { get; set; }
        [ValidarGarantia]
        public string garantia_causa { get; set; }
        [ValidarGarantia]
        public string garantia_solucion { get; set; }
        [VerificarRazonIngreso]
        public string fecha_soat { get; set; }
        [VerificarRazonIngreso]
        public string numero_soat { get; set; }
        public int? recibidode { get; set; }

        public int? id_plan_mantenimiento { get; set; }

        public int? Estadodos { get; set; }

        public class VerificarRazonIngreso : ValidationAttribute
        {
            private readonly Iceberg_Context context = new Iceberg_Context();

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                EncabezadoOTModel modelo = (EncabezadoOTModel)validationContext.ObjectInstance;


                trazonesingreso nombre = context.trazonesingreso.Where(d => d.id == modelo.razoningreso).FirstOrDefault();
                if (nombre != null)
                {
                    if (nombre.razoz_ingreso.ToUpper().Contains("COLISION"))
                    {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                        {
                            return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                        }
                    }

                }
                return ValidationResult.Success;
            }
        }

        public class ValidarGarantia : ValidationAttribute
        {
            private readonly Iceberg_Context context = new Iceberg_Context();

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                EncabezadoOTModel modelo = (EncabezadoOTModel)validationContext.ObjectInstance;


                trazonesingreso nombre = context.trazonesingreso.Where(d => d.id == modelo.razoningreso).FirstOrDefault();
                if (nombre != null)
                {
                    if (nombre.razoz_ingreso.ToUpper().Contains("GARAN"))
                    {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                        {
                            return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                        }
                    }

                }
                return ValidationResult.Success;
            }
        }
    }


}