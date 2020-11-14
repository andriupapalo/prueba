using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Homer_MVC.IcebergModel
{
    public class icb_vehiculo_eventosModel
    {
        public int evento_id { get; set; }
        public string planmayor { get; set; }
        public int? eventoid_licencia { get; set; }
        public DateTime eventofec_creacion { get; set; }
        public int? eventouserid_creacion { get; set; }
        public DateTime? eventofec_actualizacion { get; set; }
        public int? eventouserid_actualizacion { get; set; }
        public string evento_nombre { get; set; }
        public bool evento_estado { get; set; }
        public string eventorazoninactivo { get; set; }
        public int? bodega_id { get; set; }
        public int? idtecnico { get; set; }
        public string id_tpevento { get; set; }
        public string evento_observacion { get; set; }

        [Display(Name = "fecha")] [Required] public DateTime fechaevento { get; set; }

        [Display(Name = "ubicacion")]
        [ValidaUbicacion]
        public int? ubicacion { get; set; }

        public int? terceroid { get; set; }
        public string razon_inactivo { get; set; }

        [Display(Name = "placa")]
        [ValidaPlaca]
        public string placa { get; set; }

        public string vin { get; set; }

        [Display(Name = "vin")] [ValidaNit] public string nit { get; set; }

        public string poliza { get; set; }

        public class ValidaPlaca : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_vehiculo_eventosModel evento = (icb_vehiculo_eventosModel)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    icb_tpeventos buscarEvento =
                        context.icb_tpeventos.FirstOrDefault(x => x.tpevento_id.ToString() == evento.id_tpevento);
                    if (buscarEvento != null)
                    {
                        if (buscarEvento.pplaca)
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName +
                                                            " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaNit : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_vehiculo_eventosModel evento = (icb_vehiculo_eventosModel)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    icb_tpeventos buscarEvento =
                        context.icb_tpeventos.FirstOrDefault(x => x.tpevento_id.ToString() == evento.id_tpevento);
                    if (buscarEvento != null)
                    {
                        if (buscarEvento.ptercero)
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName +
                                                            " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaUbicacion : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_vehiculo_eventosModel evento = (icb_vehiculo_eventosModel)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    icb_tpeventos buscarEvento =
                        context.icb_tpeventos.FirstOrDefault(x => x.tpevento_id.ToString() == evento.id_tpevento);
                    if (buscarEvento != null)
                    {
                        if (buscarEvento.pubicacion)
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()))
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName +
                                                            " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}