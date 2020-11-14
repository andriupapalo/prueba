using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class FacturaVehiculoModel
    {
        [Display(Name = "tipo")]
        [ValidaTipoDocumento]
        public int tipo { get; set; }

        public int? id_tercero { get; set; }

        [Display(Name = "nit")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string nit { get; set; }

        [Display(Name = "forma pago")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int fpago_id { get; set; }

        [Display(Name = "bodega")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int bodega { get; set; }

        public long? pedido { get; set; }
        public int? concepto { get; set; }
        public int? concepto2 { get; set; }
        public string conceptoLetras { get; set; }
        public string concepto2Letras { get; set; }

        [Display(Name = "primer nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string primerNombre { get; set; }

        public string segundoNombre { get; set; }
        public string primerApellido { get; set; }
        public string segundoApellido { get; set; }
        public string direccion { get; set; }
        public string celular { get; set; }

        [Display(Name = "perfil contable")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int perfilContable { get; set; }

        [Display(Name = "plan mayor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string planMayor { get; set; }

        [Display(Name = "modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string modeloVh { get; set; }

        /************************************************************************************************/
        [Display(Name = "marca")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string marcaVh { get; set; }

        [Display(Name = "numero de serie")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string numSerie { get; set; }

        [Display(Name = "color")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string colorVh { get; set; }

        public string tipoCajaVh { get; set; }
        public string tipoMotorVh { get; set; }
        public string numeroMotor { get; set; }
        public string cilindraje { get; set; }
        public string capacidad { get; set; }
        public string carroceriaVh { get; set; }
        public string claseVh { get; set; }
        public string servicioVh { get; set; }

        [Display(Name = "año modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime anioModeloVh { get; set; }

        public string manifestoVh { get; set; }
        public string fechaVh { get; set; }

        public string ciudadVh { get; set; }

        /************************************************************************************************/
        [Display(Name = "precio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string precio { get; set; }

        [Display(Name = "iva")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal iva { get; set; }

        [Display(Name = "valor iva")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string valorIva { get; set; }

        [Display(Name = "impuesto consumo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal impuestoConsumo { get; set; }

        [Display(Name = "impuesto consumo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string valorImpuestoConsumo { get; set; }

        [Display(Name = "valor total")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string valorTotal { get; set; }

        public long? numero { get; set; }

        [Display(Name = "vendedor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int vendedor { get; set; }

        public string retencion { get; set; }
        public string retencion_iva { get; set; }
        public string retencion_ica { get; set; }
        public string nota { get; set; }
        public int areaIngreso { get; set; }
        public int? encabDoc { get; set; }

        public class ValidaTipoDocumento : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                FacturaVehiculoModel factura = (FacturaVehiculoModel)validationContext.ObjectInstance;

                if (factura.tipo == 0)
                {
                    return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                }

                return ValidationResult.Success;
            }
        }
    }
}