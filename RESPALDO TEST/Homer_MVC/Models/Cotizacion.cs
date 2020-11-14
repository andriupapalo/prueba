using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Homer_MVC.IcebergModel
{
    public class Cotizacion
    {
        public int cotizacion_id { get; set; }
        public int? tercero_id { get; set; }

        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tpdoc_id { get; set; }

        [Display(Name = "documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string doc_tercero { get; set; }
        public string digito_verificacion { get; set; }

        [Display(Name = "primer nombre")]
        [ValidaSiRequierePrimerNombre]
        public string prinom_tercero { get; set; }

        public string segnom_tercero { get; set; }

        [Display(Name = "primer apellido")]
        [ValidaSiRequierePrimerApellido]
        public string apellido_tercero { get; set; }

        public string segapellido_tercero { get; set; }


        [Display(Name = "razon social")]
        [ValidaSiRequiereRazonSocial]
        public string razon_social { get; set; }
        [Display(Name = "celular")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string celular_tercero { get; set; }
        public string telefono_tercero { get; set; }

        [Display(Name = "direccion")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string direc_tercero { get; set; }

        [Display(Name = "ciudad")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int ciu_id { get; set; }
        [Display(Name = "departamento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int dpto_id { get; set; }
        public string modvh_codigo { get; set; }
        public int? anio { get; set; }
        public string colVh_id { get; set; }
        public string valor { get; set; }
        public string descripcion { get; set; }
        public int fpago_id { get; set; }
        public bool envio_correo { get; set; }
        public string costo_poliza { get; set; }
        public int aseg_id { get; set; }
        public string matricula { get; set; }
        public string observacion { get; set; }

        [Display(Name = "fuente")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tporigen_id { get; set; }
        [ValidaSiRequiereSubFuente]
        [Display(Name = "Sub-Fuente")]
        public int? subfuente_id { get; set; }
        [Display(Name = "medio comunicación")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int medcomun_id { get; set; }
        [Display(Name = "correo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string correo { get; set; }
        [Display(Name = "genero")]
        [ValidaSiRequiereGenero]
        public int? gentercero_id { get; set; }
        public int? ocupacion { get; set; }
        public string cot_valortotal { get; set; }
        public int idplan_pago { get; set; }
        public int financiera_id { get; set; }
        public int valor_total { get; set; }
        public int cuota_inicial { get; set; }
        public int? marcvh_id { get; set; }
        public bool habdtautor_correo { get; set; }
        public bool habdtautor_celular { get; set; }
        public bool habdtautor_msm { get; set; }
        public bool habdtautor_whatsapp { get; set; }
        [Display(Name = "asesor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int asesorAsignado { get; set; }

        [Display(Name = "evento")]
        [ValidaEventoOrigen]
        public int? eventoOrigen { get; set; }

        public int? listadovehiculos { get; set; }
        public int? listadoaccesorios { get; set; }
        public int? listadoretomas { get; set; }
        public List<ListaAccesorios> accesorios { get; set; }
        public List<ListaVehiculos> vehiculos { get; set; }
        public List<ListaRetomas> retomas { get; set; }


        public class ValidaSiRequiereRazonSocial : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_documento buscarSiEsNit = context.tp_documento.FirstOrDefault(x => x.tpdoc_id == cotizacion.tpdoc_id);
                    if (buscarSiEsNit != null)
                    {
                        if (buscarSiEsNit.tpdoc_nombre.ToUpper().Contains("NIT"))
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaSiRequiereSubFuente : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_origen buscarSiEsVitrina = context.tp_origen.FirstOrDefault(x => x.tporigen_id == cotizacion.tporigen_id);
                    if (buscarSiEsVitrina != null)
                    {
                        if (buscarSiEsVitrina.subfuente == true)
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaSiRequiereGenero : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_documento buscarSiEsNit = context.tp_documento.FirstOrDefault(x => x.tpdoc_id == cotizacion.tpdoc_id);
                    if (buscarSiEsNit != null)
                    {
                        if (!buscarSiEsNit.tpdoc_nombre.ToUpper().Contains("NIT"))
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                            }
                        }
                    }
                    else if (cotizacion.tpdoc_id == 0)
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaSiRequierePrimerNombre : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_documento buscarSiEsNit = context.tp_documento.FirstOrDefault(x => x.tpdoc_id == cotizacion.tpdoc_id);
                    if (buscarSiEsNit != null)
                    {
                        if (!buscarSiEsNit.tpdoc_nombre.ToUpper().Contains("NIT"))
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                            }
                        }
                    }
                    else if (cotizacion.tpdoc_id == 0)
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaSiRequierePrimerApellido : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_documento buscarSiEsNit = context.tp_documento.FirstOrDefault(x => x.tpdoc_id == cotizacion.tpdoc_id);
                    if (buscarSiEsNit != null)
                    {
                        if (!buscarSiEsNit.tpdoc_nombre.ToUpper().Contains("NIT"))
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                            }
                        }
                    }
                    else if (cotizacion.tpdoc_id == 0)
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaEventoOrigen : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Cotizacion cotizacion = (Cotizacion)validationContext.ObjectInstance;

                if (cotizacion.tporigen_id == 2)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #region Retomas

        public decimal? valorRetoma { get; set; }
        public string placaRetoma { get; set; }
        public string modeloRetoma { get; set; }
        public decimal? kilometrajeRetoma { get; set; }
        public int? anioVehiculoRetoma { get; set; }

        #endregion
    }


    public class ListaVehiculos
    {
        public string descripcion { get; set; }
        public string aniovehiculo { get; set; }
    }

    public class ListaAccesorios
    {

    }

    public class ListaRetomas
    {

    }
}