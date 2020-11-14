using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PedidoCotizacionPDFModel
    {
        #region Bodega

        public string NombreBodega { get; set; }
        public string DireccionBodega { get; set; }
        public string CiudadBodega { get; set; }
        public string DepartamentoBodega { get; set; }

        #endregion

        #region Cliente

        public long NumeroPedido { get; set; }
        public string DiaPedido { get; set; }
        public string MesPedido { get; set; }
        public string AnioPedido { get; set; }
        public string NombresCliente { get; set; }
        public string CedulaNit { get; set; }
        public string DireccionCliente { get; set; }
        public string CiudadCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string ProfesionCliente { get; set; }
        public string EmailCliente { get; set; }
        public string EstadoCivil { get; set; }
        public string FechaNacimientoCliente { get; set; }
        public string CelularCliente { get; set; }
        public string ReferidoPor { get; set; }
        public string NombresAsegurados { get; set; }
        public string DocumentosAsegurados { get; set; }

        #endregion

        #region Vehiculo

        public string ModeloVehiculo { get; set; }
        public int AnioVehiculo { get; set; }
        public string ColorVehiculo { get; set; }
        public string TipoVehiculo { get; set; }
        public string PlacaVehiculo { get; set; }
        public string PlanMayor { get; set; }
        public string ServicioVehiculo { get; set; }
        public string ModeloVhRetoma { get; set; }
        public int AnioVhRetoma { get; set; }
        public string ColorVhRetoma { get; set; }
        public string TipoVhRetoma { get; set; }
        public string PlacaVhRetoma { get; set; }
        public string ValorRetoma { get; set; }

        #endregion

        #region Precio

        public string PrecioAlPublico { get; set; }
        public string financiera { get; set; }
        public string Descuento { get; set; }
        public string PrecioVenta { get; set; }
        public string Accesorios { get; set; }
        public string TotalAccesorios { get; set; }
        public string valorCredito { get; set; }

        public string CuotaInicial { get; set; }
        public string ValorRetomaPago { get; set; }
        public string SaldoFinanciar { get; set; }
        public string Total { get; set; }

        #endregion

        #region Vendedor

        public string CedulaVendedor { get; set; }
        public string Vendedor { get; set; }
        public string PrendaAFavorDe { get; set; }
        public string CedulaAprobado { get; set; }
        public string NombreAprobado { get; set; }
        #endregion

        //modificaciones laura

        #region Credito

        public string cantCuotas { get; set; }
        public string saldoFinanciar { get; set; }
        public string poliza { get; set; }
        public string cuoInicial { get; set; }

        #endregion
    }



}