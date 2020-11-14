using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class NoteDebitScheduleModel
    {
        /// <summary>
        ///     Se crea una clase nueva con la intención de crear un modal que me permita llenarlo de forma rápida y concisa, el
        ///     objetivo de esta clase es definir, los atributos de nota debito cliente
        ///     para mayor referencias sobre nota debito cliente, dirijase al create de nota debito cliente para los atributos que
        ///     estoy tomando y que estoy definiendo en esta clase,
        ///     despues crearé una función dentro del controlador de Vpedidos para definir los datos y llenarlos, posteriormente
        ///     crearé un form de tipo HTML con su respectivo nombre de "NoteDebitModal.html" para
        ///     definir los datos que necesito, a continuación crearé una función para validar los datos para cargar el modal que
        ///     será de vista parcial.
        /// </summary>
        public int Id { get; set; }

        public int? motive { get; set; }
        public bool estadoMatrc { get; set; }

        [Display(Name = " Tipo documento Contable ")]
        public int typeDocument { get; set; }

        public long IdDocument { get; set; }

        [Display(Name = " Nombre del cliente ")]
        public string NameClient { get; set; }

        public int IdLarder { get; set; }

        [Display(Name = " Bodega ")] public string NameLarder { get; set; }

        [Display(Name = " Nit / Cedula ")] public long IdNit { get; set; }

        [Required] public int IdAgent { get; set; }

        [Display(Name = " Vendedor ")] public string NameAgent { get; set; }

        [Display(Name = " Perfil Contable ")] public long CountableProfile { get; set; }

        public string CountablenNameProfile { get; set; }

        [Display(Name = " Observaciones ")] public string Observation { get; set; }

        // solo falta el valor factura en el controlador
        [Display(Name = " Valor Factura ")] public int ReceiptValue { get; set; }

        [Display(Name = " Valor ")] public int Value { get; set; }

        [Display(Name = " Total ")] public int? Total { get; set; }

        public int? Prefix { get; set; }

        public string Document { get; set; }

        public int id_head { get; set; }

        public int? receiptType { get; set; }

        [Display(Name = " validar Matricula ")]
        public bool licensePlate { get; set; }

        [Display(Name = " Placa ")] public string licensePlatename { get; set; }

        [Display(Name = " Fecha de preregistro ")]
        public string pregisDateLic { get; set; }

        [Display(Name = " Fecha de Matricula")]
        public string licplaDate { get; set; }

        [Display(Name = "Tramitador")] public int tramitador { get; set; }
        [Display(Name = "Fecha de tramite")] public string tramiteDate { get; set; }
        [Display(Name = "# Pedido")] public int? orderNumber { get; set; }
        [Display(Name = "Id pedido")] public int IdOrder { get; set; }
    }
}