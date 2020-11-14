using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class VehiculoPedidoModel
    {
        public int id { get; set; }
        public int bodega { get; set; }
        public int? numero { get; set; }
        public int? idcotizacion { get; set; }
        [Display(Name = "cliente / nit")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? nit { get; set; }
        public int? nit_asegurado { get; set; }
        public int? nit2 { get; set; }
        public int? nit3 { get; set; }
        public int? nit4 { get; set; }
        public int? nit5 { get; set; }
        public bool impfactura2 { get; set; }
        public bool impfactura3 { get; set; }
        public bool impfactura4 { get; set; }
        [Display(Name = "asesor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? vendedor { get; set; }
        [Display(Name = "modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string modelo { get; set; }
        [Display(Name = "marca")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string marcvh_id { get; set; }
        public System.DateTime fecha { get; set; }
        [Display(Name = "año modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? id_anio_modelo { get; set; }
        public int? plan_venta { get; set; }
        public string planmayor { get; set; }
        public Nullable<System.DateTime> fecha_asignacion_planmayor { get; set; }
        public int? asignado_por { get; set; }
        public int? condicion { get; set; }
        public int? dias_validez { get; set; }
        public string valor_unitario { get; set; }
        public Nullable<float> porcentaje_iva { get; set; }
        public string valorPoliza { get; set; }
        public Nullable<float> pordscto { get; set; }
        public string vrdescuento { get; set; }
        public int? cantidad { get; set; }
        public int? tipo_carroceria { get; set; }
        public string vrcarroceria { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")] public string vrtotal { get; set; }
        public bool anulado { get; set; }
        public int? moneda { get; set; }
        public int? id_aseguradora { get; set; }
        public string notas1 { get; set; }
        public string notas2 { get; set; }
        public bool escanje { get; set; }
        public bool eschevyplan { get; set; }
        public bool esreposicion { get; set; }
        public bool esLeasing { get; set; }
        public int? nit_prenda { get; set; }
        public int? flota { get; set; }
        public bool facturado { get; set; }
        public int? numfactura { get; set; }
        public Nullable<float> porcentaje_impoconsumo { get; set; }
        public string numeroplaca { get; set; }
        public string motivo_anulacion { get; set; }
        public bool venta_gerencia { get; set; }
        public string Color_Deseado { get; set; }
        public int? terminacionplaca { get; set; }
        public Nullable<decimal> bono { get; set; }
        public int? idmodelo { get; set; }
        public bool nuevo { get; set; }
        public Nullable<bool> usado { get; set; }
        [Display(Name = "servicio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? servicio { get; set; }
        public bool placapar { get; set; }
        public bool placaimpar { get; set; }
        public string color_opcional { get; set; }
        public int? cargomatricula { get; set; }
        public Nullable<float> obsequioporcen { get; set; }
        public string valormatricula { get; set; }
        public string rango_placa { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public Nullable<int> codflota { get; set; }
        public Nullable<int> iddepartamento { get; set; }
        public Nullable<int> idciudad { get; set; }
        public string color { get; set; }
        public string serie { get; set; }
        public string valorsoat { get; set; }
        public string otrosValores { get; set; }

        /**********************Campos de cambio vehiculo******************************/
        public string marcas { get; set; }
        public string modelosMarca { get; set; }
        public int? idAnioModelo { get; set; }

        public string motivoCambio { get; set; }
        /*****************************************************************************/

        public int numerocotizacion { get; set; }
        public string numeroIdentificacion { get; set; }
    }
}