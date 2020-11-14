using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class PDFmodel
    {
        public string tipoEntrada { get; set; }
        public int idcotizacion { get; set; }
        public int idtercero { get; set; }
        public long numeroRegistro { get; set; }
        public string fechaRegistro { get; set; }
        //public List<docCruzado> docCruzado { get; set; }
        public string prefijoCruzado { get; set; }
        public string numeroCruzado { get; set; }
        public string nombreBodega { get; set; }
        public decimal TotalTotales { get; set; }
        public string notas { get; set; }
        public List<referenciasPDF> referencias { get; set; }
        public List<documento> cuentas { get; set; }
        public int destinoID { get; set; }
        public string bodegaOrigen { get; set; }
        public string BodegaDestino { get; set; }
        public string ubicacion { get; set; }
        public string Direccion { get; set; }

        //----------------------------------------------------------------Recibo caja-------------------------------------------------------------------------------

        public string nombreTercero { get; set; }
        public string Idtercero { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string prefijo { get; set; }
        public string ValorEncabezadoTotal { get; set; }
        public string fechaEncabezado { get; set; }
        public string fechaTabla { get; set; }
        public string responsable { get; set; }
        public string bodega { get; set; }

        //Los siguientes campos serán usados para la consulta en notacredito

        public string fechaEncab { get; set; }
        public long numeroEntrada { get; set; }
        public string ciudad { get; set; }
        public string vendedor { get; set; }
        public string detalleConcepto { get; set; }
        public string valorTotal { get; set; }
        public string iva { get; set; }
        public string descuento { get; set; }
        public string subtotal { get; set; }
        public string totalfinal { get; set; }

        //Los siguientes campos serán usados para la consulta en LibroMayor

        public int anio_cuenta { get; set; }
        public string mes_cuenta { get; set; }
        public string nombre_libro { get; set; }
        public decimal total_saldoInicial { get; set; }
        public decimal total_debito { get; set; }
        public decimal total_credito { get; set; }
        public decimal total_saldoActual { get; set; }

        //Los siguientes campos serán usados para la consulta en generar pdf de CotizacionController

        public string nombreCompletoTercero { get; set; }
        public string tipoDocumento { get; set; }
        public string numeroDocumento { get; set; }
        public string correo { get; set; }
        public string celular { get; set; }
        public string fechaCreacion { get; set; }
        public string numeroCotizacion { get; set; }
        public string nombreAsesor { get; set; }
        public string telefono_asesor { get; set; }
        public string correo_asesor { get; set; }
        public string telefono_cliente { get; set; }
        public string celular_cliente { get; set; }
        public string direccion_cliente { get; set; }
        public string correo_cliente { get; set; }
        public string notasCotizacion { get; set; }
        public string digitoVerificacion { get; set; }

        //----------------------------------------------------Devolución Compras-----------------------------------------//

        public string empresa { get; set; }
        public string numeroDevolucion { get; set; }
        public string numeroFactura { get; set; }
        public string proveedor { get; set; }
        public string nit { get; set; }
        public float? totalDescuento { get; set; }
        public decimal fletes { get; set; }

        //----------------------------------------------------Cotización Digital-----------------------------------------//

        public string vehiculo { get; set; }
        public int? anioModelo { get; set; }
        public string color { get; set; }
        public decimal? matricula { get; set; }
        public decimal? soat { get; set; }
        public decimal? poliza { get; set; }
        public string imgPrincipal { get; set; }
        public string imgDetalle1 { get; set; }
        public string imgDetalle2 { get; set; }
        public string imgDetalle3 { get; set; }
        public string texto1 { get; set; }
        public string pieFoto { get; set; }
        public string[] tituloDet1 { get; set; }
        public string palabraResaltada1 { get; set; }
        public string[] tituloDet2 { get; set; }
        public string palabraResaltada2 { get; set; }
        public string[] tituloDet3 { get; set; }
        public string palabraResaltada3 { get; set; }
        public string cuerpoTitulo1 { get; set; }
        public string cuerpoTitulo2 { get; set; }
        public string cuerpoTitulo3 { get; set; }
        public int? chevyStar { get; set; }
        public string chevyStarImg { get; set; }
        public decimal valorConDescuento { get; set; }
        public decimal valorRetoma { get; set; }
        public decimal valorAccesorios { get; set; }
        public decimal precioFinanciacion { get; set; }
        public decimal credito { get; set; }
        public decimal cuotas { get; set; }
        public decimal cuotaInicial { get; set; }
        public decimal valorResidual { get; set; }
        public decimal otrosValores { get; set; }
        public decimal tasaInteres { get; set; }
        public List<planmatriz> planes { get; set; }
    }

    public class docCruzado {
        public string prefijo { get; set; }
        public long numero { get; set; }
    }
    public class referenciasPDF
    {
        public decimal soat { get; set; }
        public string ubicacion { get; set; }
        public decimal cantidad { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal costo { get; set; }
        public decimal total { get; set; }
        public int secuencia { get; set; }

        //Los siguientes campos serán usados para la consulta en recibocaja
        public string observacion { get; set; }
        public string nombreBanco { get; set; }
        public string tipoPagoRecibido { get; set; }
        public decimal valorInicial { get; set; }

        //Los siguientes campos serán usados para la consulta en notacredito

        public long numeroFactura { get; set; }
        public decimal valorFactura { get; set; }
        public string prefijo { get; set; }

        //Los siguientes campos serán usados para la consulta en LibroMayor
        public string numero_cuenta { get; set; }
        public string descripcion_cuenta { get; set; }
        public decimal credito { get; set; }
        public decimal debito { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal saldo_actual { get; set; }
        public string prefijo_libro_mayor { get; set; }
        public string nombre_cuenta_libro_mayor { get; set; }

        //-------------------------------------------------------------------------------------
        //Los siguientes campos serán usados para la consulta en generar pdf de CotizacionController

        public string vehiculo { get; set; }
        public Nullable<decimal> precio { get; set; }
        public Nullable<decimal> matricula { get; set; }
        public Nullable<decimal> seguro { get; set; }



        public List<planesDePago> planesDePago { get; set; }


        //------------------------------------------------ Devolución Compras ---------------------------------------//

        public float? iva { get; set; }
        public float? descuento { get; set; }
        public decimal costoTotal { get; set; }
        public string notas { get; set; }
        public decimal fletes { get; set; }

    }

    public class planesDePago
    {
        public string nombrePlan { get; set; }
        public decimal? cuotaInicial { get; set; }
        public decimal? financiacion { get; set; }
        public string tasa { get; set; }
        public List<plazosYcoutas> plazosYcoutasCotizacion { get; set; }

    }
    public class plazosYcoutas
    {
        public string plazo { get; set; }
        public decimal valor { get; set; }
    }
    public class documento
    {
        public int ano { get; set; }
        public int mes { get; set; }
        public string nombre_documento { get; set; }
        public string prefijo { get; set; }
        public decimal total_credito { get; set; }
        public decimal total_debito { get; set; }
        public decimal total_creniff { get; set; }
        public decimal total_deniff { get; set; }
        public List<cuenta> cuentas { get; set; }

    }
    public class cuenta
    {
        public int ano { get; set; }
        public int mes { get; set; }
        public string numerocuenta { get; set; }
        public string cntpuc_descp { get; set; }
        public decimal debito_cuenta { get; set; }
        public decimal credito_cuenta { get; set; }
        public decimal debnif_cuenta { get; set; }
        public decimal crednif_cuenta { get; set; }


    }

    public class planmatriz
    {
        public int llave { get; set; }
        public int cuenta { get; set; }
        public List<planes_pago> plan { get; set; }

    }
    public class planes_pago
    {
        public string plazo { get; set; }
        public decimal valor { get; set; }
    }

}