using System.ComponentModel.DataAnnotations;


namespace Homer_MVC.Models
{
    public class formularioliquidacionOT
    {
        [Display(Name = "Codigo Orden")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string codigoentrada { get; set; }
        public string correo { get; set; }
        public string direccion { get; set; }
        [Required]
        public int? forma_pago_id { get; set; }
        public string documento { get; set; }
        [Display(Name = "id")]
        public int? idorden { get; set; }
        [Display(Name = "Tipo Documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? tipoDocumento { get; set; }

        [Display(Name = "Perfil Contable")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public int? perfilContable { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public string nombre { get; set; }
        [Display(Name = "Placa / Plan Mayor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string placa { get; set; }
        [Display(Name = "Razon de Ingreso")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? razon_ingreso { get; set; }
        public int? razon_ingreso2 { get; set; }
        [Display(Name = "Tecnico")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? tecnico { get; set; }

        [Display(Name = "Cartera")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? cartera { get; set; }
        public string forma_pago { get; set; }
        public string telefono { get; set; }
        public string celular { get; set; }

        public int? id_cliente { get; set; }
        public int? bodega { get; set; }
        public string kilometraje { get; set; }
        public string serie { get; set; }
        public string motor { get; set; }
        public string color { get; set; }
        public string fecha_venta { get; set; }
        public string fecha_fin_garantia { get; set; }
        public string asesor { get; set; }
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
        public bool garantia { get; set; }

    }
}