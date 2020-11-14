using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Homer_MVC.IcebergModel
{
    public class CrearVehiculoModel
    {
        public int icbvh_id { get; set; }

        //[Display(Name = "plan mayor")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string plan_mayor { get; set; }

        //[Display(Name = "modelo")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string modvh_id { get; set; }

        //[Display(Name = "color")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string colvh_id { get; set; }

        //[Display(Name = "serie")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string vin { get; set; }

        //[Display(Name = "numero motor")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nummot_vh { get; set; }

        //[Display(Name = "año")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int anio_vh { get; set; }

        //[Display(Name = "tipo")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int? tpvh_id { get; set; }

        //[Display(Name = "marca")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int marcvh_id { get; set; }

        public string plac_vh { get; set; }

        //[Display(Name = "costo total")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string costototal_vh { get; set; }

        //[Display(Name = "costo")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string costosiniva_vh { get; set; }

        //[Display(Name = "iva")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        // public Nullable<decimal> iva_vh { get; set; }
        public string iva_vh { get; set; }

        //[Display(Name = "impuesto consumo")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //public Nullable<decimal> impconsumo { get; set; }
        public string iva_compra_vh { get; set; }
        public string impconsumo { get; set; }
        public string Notas { get; set; }
        public bool flota { get; set; }
        public bool nuevo { get; set; }

        public int? proveedor_id { get; set; }

        //public Nullable<int> propietario_id { get; set; }
        public int? aseguradora_id { get; set; }
        //public DateTime? fecfact_fabrica { get; set; }
        //public DateTime? fecentman_vh { get; set; }
        public string fecfact_fabrica { get; set; }
        public string fecentman_vh { get; set; }
        public string fechaentrega { get; set; }
        public string nombreconcesionario { get; set; }
        public string nummanf_vh { get; set; }
        public int? ciumanf_vh { get; set; }
        public int? diaslibres_vh { get; set; }
        public int? icbvhid_licencia { get; set; }
        //public DateTime icbvhfec_creacion { get; set; }
        public DateTime icbvhfec_creacion { get; set; }
        public int icbvhuserid_creacion { get; set; }
        public DateTime? icbvhfec_actualizacion { get; set; }
        public int? icbvhuserid_actualizacion { get; set; }
        public int? tipo_servicio { get; set; }
        public bool usado { get; set; }
        public string Numerogarantia { get; set; }
        //public DateTime? fecha_garantia { get; set; }
        public string fecha_garantia { get; set; }

        public int? tiempogarantia { get; set; }
        public string kmgarantia { get; set; }
        public string kilometraje { get; set; }
        public string numerosoat { get; set; }

        [ValidaNitAseguradora] public string nitaseguradora { get; set; }

        [ValidaPropietario] public string propietario { get; set; }

        //public DateTime? fecha_venta { get; set; }
        //public DateTime? fecha_fin_garantia { get; set; }
        //public DateTime? fecha_tecnomecanica { get; set; }
        //public DateTime? fecha_matricula { get; set; }
        public string fecha_venta { get; set; }
        public string fecha_fin_garantia { get; set; }
        public string fecha_tecnomecanica { get; set; }
        public string fecha_matricula { get; set; }
        public string codigo_pago { get; set; }
        public int? ubicacion { get; set; }
        public string nombreUbicacion { get; set; }
        public string nombreUltimaUbicacion { get; set; }
        public int? ciudadplaca { get; set; }
        public int? nitprenda { get; set; }
        public int? idasesor { get; set; }
        public bool icbvh_estado { get; set; }
        public string icbvhrazoninactivo { get; set; }


        //public DateTime? fecha_soat { get; set; }
        public string fecha_soat { get; set; }

        public class ValidaNitAseguradora : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                CrearVehiculoModel vehiculo = (CrearVehiculoModel)validationContext.ObjectInstance;

                if (!string.IsNullOrEmpty(vehiculo.nitaseguradora))
                {
                    using (Iceberg_Context context = new Iceberg_Context())
                    {
                        if (value != null || string.IsNullOrEmpty(value.ToString()))
                        {
                            string nit = value.ToString();
                            icb_terceros buscarNit = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == nit);
                            if (buscarNit == null)
                            {
                                return new ValidationResult("El nit no se encontro");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaPropietario : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                CrearVehiculoModel vehiculo = (CrearVehiculoModel)validationContext.ObjectInstance;

                if (!string.IsNullOrEmpty(vehiculo.propietario))
                {
                    using (Iceberg_Context context = new Iceberg_Context())
                    {
                        if (value != null || string.IsNullOrEmpty(value.ToString()))
                        {
                            int nit = Convert.ToInt32(value);
                            icb_terceros buscarNit = context.icb_terceros.FirstOrDefault(x => x.tercero_id == nit);
                            if (buscarNit == null)
                            {
                                return new ValidationResult("El documento no se encontro");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}