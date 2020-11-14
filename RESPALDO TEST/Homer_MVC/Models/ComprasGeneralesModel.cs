using System;

namespace Homer_MVC.Models
{
    public class ComprasGeneralesModel
    {
        //IMPORTANTE: Este modelo se utiliza para grabar los 4 tipos de notas contables
        //1) Nota credito cliente, 2) Nota debito cliente, 3) Nota credito proveedor, 4) Nota debito proveedor
        //en este modelo se encuentran las 4 tablas en las que se debe grabar informacion: Encabezado documento, movimiento contable, cruce documentos y cuentas valores

        //IMPORTANTE: Se añadieron las tablas de lineas documento y referencias por el motivo de que este mismo modelo sera utilizado en la admionistracion de compra manual de repuestos

        //Inicio Encabezado
        #region encabezado        
        public int idencabezado { get; set; }
        public int tipo { get; set; }
        public long numero { get; set; }
        public int nit { get; set; }
        public System.DateTime fecha { get; set; }
        public int? fpago_id { get; set; }
        public Nullable<System.DateTime> vencimiento { get; set; }
        public string valor_total { get; set; }
        public string iva { get; set; }
        public string retencion { get; set; }
        public string retencion_causada { get; set; }
        public string retencion_iva { get; set; }
        public string retencion_ica { get; set; }
        public string descuento_pie { get; set; }
        public string fletes { get; set; }
        public string iva_fletes { get; set; }
        public string valorFactura { get; set; }
        public int? vendedor { get; set; }
        public string valor_aplicado { get; set; }
        public bool anulado { get; set; }
        public string numFactura { get; set; }
        public string notas { get; set; }
        public int bodega { get; set; }
        public decimal impoconsumo { get; set; }
        public Nullable<int> concepto { get; set; }
        public Nullable<int> prefijo { get; set; }
        public Nullable<int> moneda { get; set; }
        public Nullable<int> centro_doc { get; set; }
        public string valor_mercancia { get; set; }
        public Nullable<int> id_pedido_vehiculo { get; set; }
        public string nota1 { get; set; }
        public string nota2 { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string modelo { get; set; }
        public Nullable<int> nit_pago { get; set; }
        public string numorden { get; set; }
        public Nullable<long> pedido { get; set; }
        public Nullable<decimal> tasa { get; set; }
        public string por_retencion { get; set; }
        public string por_retencion_iva { get; set; }
        public string por_retencion_ica { get; set; }
        public Nullable<int> concepto2 { get; set; }
        public Nullable<int> facturado_a { get; set; }
        public string tipo_facturadoa { get; set; }
        public Nullable<int> destinatario { get; set; }
        public Nullable<int> bodega_destino { get; set; }
        public Nullable<int> perfilcontable { get; set; }
        public Nullable<int> motivocompra { get; set; }
        public Nullable<int> solicitadopor { get; set; }
        public bool anticipo { get; set; }
        public int? tipofactura { get; set; }

        public string prefijoFac { get; set; }

        public int retencion_id { get; set; }

        #endregion
        //Fin Encabezado

        //Movimiento contable
        #region movimiento contable
        public int idmov { get; set; }
        public int id_encab { get; set; }
        public int seq { get; set; }
        public Nullable<int> idparametronombre { get; set; }
        public int cuenta { get; set; }
        public Nullable<int> centro { get; set; }
        public System.DateTime fec { get; set; }
        public Nullable<decimal> debito { get; set; }
        public Nullable<decimal> credito { get; set; }
        public Nullable<decimal> basecontable { get; set; }
        public Nullable<decimal> debitoniif { get; set; }
        public Nullable<decimal> creditoniif { get; set; }
        public string detalle { get; set; }
        #endregion
        //Fin Movimiento contable

        /// Otros campos
        public int ctaimpuesto { get; set; }
        public int ctaretencion { get; set; }
        public int ctareteiva { get; set; }
        public int ctaica { get; set; }
        public int ctaxpagar { get; set; }

        /// 



        //Cruce documentos
        #region cruce documentos
        //public int id { get; set; }
        //public string idtipo { get; set; }
        ////public long numero { get; set; }
        //public int idtipoaplica { get; set; }
        //public long numeroaplica { get; set; }
        //public decimal valor { get; set; }
        //public System.DateTime fechacruce { get; set; }
        #endregion
        //Fin cruce documentos

        //Cuentas valores
        #region cuentas valores
        public int ano { get; set; }
        public int mes { get; set; }
        public Nullable<decimal> saldo_ini { get; set; }
        public Nullable<decimal> saldo_ininiff { get; set; }
        public Nullable<decimal> debitoniff { get; set; }
        public Nullable<decimal> creditoniff { get; set; }
        #endregion
        //Fin Cuentas valores

        //Documentos Pagos
        #region documentos pagos
        public int idpago { get; set; }
        public int idtencabezado { get; set; }
        public Nullable<int> banco { get; set; }
        public int forma_pago { get; set; }
        public Nullable<int> consignar_en { get; set; }
        public string devuelto { get; set; }
        public string tipo_consignacion { get; set; }
        public Nullable<int> numero_consignacion { get; set; }
        public Nullable<System.DateTime> fecha_devolucion { get; set; }
        public string cuenta_banco { get; set; }
        public Nullable<decimal> iva_tarjeta { get; set; }
        public Nullable<int> tipo_devuelto { get; set; }
        public Nullable<int> numero_devuelto { get; set; }
        public int tercero { get; set; }
        #endregion
        //Fin Documentos Pagos

        //lineas documento
        #region lineas documento
        public int id_encabezado { get; set; }
        public string codigo { get; set; }
        public Nullable<decimal> cantidad { get; set; }
        public Nullable<float> porcentaje_iva { get; set; }
        public decimal valor_unitario { get; set; }
        public Nullable<float> porcentaje_descuento { get; set; }
        public decimal costo_unitario { get; set; }
        public string adicional { get; set; }
        public string und { get; set; }
        public decimal cantidad_und { get; set; }
        public decimal cantidad_pedida { get; set; }
        public decimal costo_unitario_sin { get; set; }
        public float cantidad_otra_und { get; set; }
        public Nullable<int> telefono { get; set; }
        public string tipo_op { get; set; }
        public Nullable<int> numero_op { get; set; }
        public Nullable<float> cantidad_devuelta { get; set; }
        public string tipo_link { get; set; }
        public Nullable<int> numero_link { get; set; }
        public Nullable<int> seq_link { get; set; }
        public Nullable<short> cantidad_dos { get; set; }
        public string serial { get; set; }
        public Nullable<float> porc_dcto_2 { get; set; }
        public Nullable<float> porc_dcto_3 { get; set; }
        public Nullable<int> Id_Documentos_lin_ped { get; set; }
        public string desc_adicional { get; set; }
        public decimal costo_niff { get; set; }
        #endregion
        //Fin lineas documento

        //referencias inven
        #region referencias inven
        public decimal can_ini { get; set; }
        public decimal can_ent { get; set; }
        public decimal can_sal { get; set; }
        public decimal cos_ini { get; set; }
        public decimal cos_ent { get; set; }
        public decimal cos_sal { get; set; }
        public decimal can_vta { get; set; }
        public decimal cos_vta { get; set; }
        public decimal val_vta { get; set; }
        public decimal can_dev_vta { get; set; }
        public decimal cos_dev_vta { get; set; }
        public decimal val_dev { get; set; }
        public decimal can_com { get; set; }
        public decimal cos_com { get; set; }
        public decimal can_dev_com { get; set; }
        public decimal cos_dev_com { get; set; }
        public decimal can_otr_ent { get; set; }
        public decimal cos_otr_ent { get; set; }
        public decimal can_otr_sal { get; set; }
        public decimal cos_otr_sal { get; set; }
        public decimal can_tra { get; set; }
        public decimal cos_tra { get; set; }
        public decimal sub_cos { get; set; }
        public decimal baj_cos { get; set; }
        public int nro_vta { get; set; }
        public int nro_dev_vta { get; set; }
        public int nro_com { get; set; }
        public int nro_dev_com { get; set; }
        public Nullable<int> nro_ped { get; set; }
        public decimal can_ped { get; set; }
        public string modulo { get; set; }
        public decimal cos_ini_aju { get; set; }
        public decimal cos_ent_aju { get; set; }
        public decimal cos_sal_aju { get; set; }
        public decimal cos_ini_niif { get; set; }
        public decimal cos_ent_niif { get; set; }
        public decimal cos_sal_niif { get; set; }
        #endregion
        //Fin referencias inven
    }
}