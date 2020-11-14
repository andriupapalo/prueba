using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Homer_MVC.IcebergModel
{
    public class v_creditosform
    {
        public int Id { get; set; }
        public int infocredito_id { get; set; }
        public int? pedido { get; set; }
        public int financiera_id { get; set; }

        public int financiera_id2 { get; set; }
        //public Nullable<System.DateTime> fec_solicitud { get; set; }
        //public Nullable<System.DateTime> fec_aprobacion { get; set; }
        //public Nullable<System.DateTime> fec_negacion { get; set; }
        //public Nullable<System.DateTime> fec_desembolso { get; set; }
        //public Nullable<System.DateTime> fec_confirmacion { get; set; }
        //public Nullable<System.DateTime> fec_envdocumentos { get; set; }
        //public Nullable<System.DateTime> fec_entdocumentos { get; set; }

        public string fec_solicitud { get; set; }
        public string fec_aprobacion { get; set; }
        public string fec_negacion { get; set; }
        public string fec_desembolso { get; set; }
        public string fec_confirmacion { get; set; }
        public string fec_envdocumentos { get; set; }
        public string fec_entdocumentos { get; set; }
        public string vsolicitado { get; set; }

        [ValidaCeroValorAprovado]
        [Display(Name = "valor aprobado")]
        public string vaprobado { get; set; }

        public string estadoc { get; set; }
        public string detalle { get; set; }
        public string num_aprobacion { get; set; }

        public int? id_licencia { get; set; }

        //public System.DateTime fec_creacion { get; set; }
        public string fec_creacion { get; set; }

        public int userid_creacion { get; set; }

        //public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public string fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public bool toma_credito { get; set; }
        public int? plazo { get; set; }
        public int? plan_id { get; set; }
        public string cuota_inicial { get; set; }
        public int? asesor_id { get; set; }
        public int? bodegaid { get; set; }

        public int? concesionarioid { get; set; }

        //public Nullable<System.DateTime> fec_desistimiento { get; set; }
        public string fec_desistimiento { get; set; }
        public int? motivodesiste { get; set; }
        public bool comison { get; set; }

        public string valor_comision { get; set; }

        //public Nullable<System.DateTime> fec_facturacomision { get; set; }
        public string fec_facturacomision { get; set; }
        public long? numfactura { get; set; }
        public string vehiculo { get; set; }
        public string vehiculo2 { get; set; }

        public int tercero { get; set; }

        //nuevo generado por jm
        public string correo_tercero { get; set; }
        public string telefono_tercero { get; set; }
        public string direccion_tercero { get; set; }
        public string precio_vehiculo { get; set; }

        public string poliza { get; set; }
    }
    //koljkojkljkljkil


    public class ValidaCeroValorAprovado : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            v_creditosform credito = (v_creditosform)validationContext.ObjectInstance;

            if (Convert.ToDecimal(credito.vaprobado, new CultureInfo("is-IS")) == 0 && credito.estadoc == "A")
            {
                return new ValidationResult("El campo " + validationContext.DisplayName + " no puede ser cero");
            }

            return ValidationResult.Success;
        }
    }
}