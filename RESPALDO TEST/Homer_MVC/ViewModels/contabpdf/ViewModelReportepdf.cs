using System;
using System.Collections.Generic;

namespace Homer_MVC.ViewModels.contabpdf
{

    public class ViewModelReportepdf
    {
        public string titulo { get; set; }
        public DateTime fechaReporte { get; set; }
        public string fechaInicio { get; set; }
        public string fechafin { get; set; }
        public string cuentaIni { get; set; }
        public string cuentaFin { get; set; }
        public decimal Grantotalsaldoini { get; set; }
        public decimal Grantotaldebito { get; set; }
        public decimal Grantotalcredito { get; set; }
        public decimal Grantotalsaldo { get; set; }
        public decimal Grantotalsaldoininiff { get; set; }
        public decimal Grantotaldebitoniff { get; set; }
        public decimal Grantotalcreditoniff { get; set; }
        public decimal Grantotalsaldoniff { get; set; }
        public List<movimientoagrupado01> movimientoagrupado01 { get; set; }
        public List<centroagrupado> centroagrupado { get; set; }
        public List<seleccion_cuenta> seleccion_cuenta { get; set; }

        public List<listaclase> listaclase { get; set; }

        //public List<movimientoagrupadocuenta> movimientoagrupadocuenta { get; set; }

        //public List<detalleCuenta> detalleCuenta { get; set; }
        //public List<Cabecera_Cuentas> Cabecera_Cuentas { get; set; }
        //public List<movimientosmes> movimientosmes { get; set; }
        //public List<movimientosmaestro> movimientosmaestro01 { get; set; }
    }



    public class detalleCuenta
    {
        public int annio_det { get; set; }
        public int mes_det { get; set; }
        public int id_cuenta_det { get; set; }
        public string numero_cuenta_det { get; set; }
        public string nombre_cuenta_det { get; set; }
        public int clase_cuenta_det { get; set; }
        public string nom_clase_cuenta_det { get; set; }
        public int grupo_cuenta_det { get; set; }
        public string nom_grupo_cuenta_det { get; set; }
        public int xcuenta_cuenta_det { get; set; }
        public int subcuenta_cuenta_det { get; set; }
        public int centro_det { get; set; }
        public int pre_centro_det { get; set; }
        public string centro_nombre_det { get; set; }
        public int tercero_det { get; set; }
        public string tercero_documento_det { get; set; }
        public string tercero_nombre_det { get; set; }
        public decimal saldoini_det { get; set; }
        public decimal debito_det { get; set; }
        public decimal credito_det { get; set; }
        public decimal saldo_det { get; set; }
        public decimal saldoininiff_det { get; set; }
        public decimal debitoniff_det { get; set; }
        public decimal creditoniff_det { get; set; }
        public decimal saldoniff_det { get; set; }
        public List<listacentro> listacentro { get; set; }
        public List<agrupa_nit> agrupa_nit { get; set; }
    }
    //public class Cabecera_Cuentas
    //{
    //    public int annio_cabecera_cts { get; set; }
    //    public int mes_cabecera_cts { get; set; }
    //    public int clase_cabecera_cts { get; set; }
    //    public string nombre_clase_cabecera_cts { get; set; }
    //    public int id_cuenta_cabecera_cts { get; set; }
    //    public string numero_cuenta_cabecera_cts { get; set; }
    //    public string nombre_cuenta_cabecera_cts { get; set; }
    //    public decimal saldoini_cabecera_cts { get; set; }
    //    public decimal debito_cabecera_cts { get; set; }
    //    public decimal credito_cabecera_cts { get; set; }
    //    public decimal saldo_cabecera_cts { get; set; }
    //    public decimal saldoininiff_cabecera_cts { get; set; }
    //    public decimal debitoniff_cabecera_cts { get; set; }
    //    public decimal creditoniff_cabecera_cts { get; set; }
    //    public decimal saldoniff_cabecera_cts { get; set; }
    //    public List<detalleCuenta> detallecuenta { get; set; }
    //}

    //**********************************
    public class centroagrupado
    {
        public int centro { get; set; }
        public string nom_centro { get; set; }
        public decimal centro_totalsaldoini { get; set; }
        public decimal centro_totaldebito { get; set; }
        public decimal centro_totalcredito { get; set; }
        public decimal centro_totalsaldo { get; set; }
        public decimal centro_totalsaldoininiff { get; set; }
        public decimal centro_totaldebitoniff { get; set; }
        public decimal centro_totalcreditoniff { get; set; }
        public decimal centro_totalsaldoniff { get; set; }
        public List<movimientoagrupado01> clases { get; set; }

    }
    public class movimientoagrupado01
    {
        public int clase { get; set; }
        public string nom_clase { get; set; }
        public int cuenta { get; set; }
        public string numero_cuenta { get; set; }
        public string nombre_cuenta { get; set; }
        public decimal Ctotalsaldoini { get; set; }
        public decimal Ctotaldebito { get; set; }
        public decimal Ctotalcredito { get; set; }
        public decimal Ctotalsaldo { get; set; }
        public decimal Ctotalsaldoininiff { get; set; }
        public decimal Ctotaldebitoniff { get; set; }
        public decimal Ctotalcreditoniff { get; set; }
        public decimal Ctotalsaldoniff { get; set; }
        public List<detalleCuenta> detalle { get; set; }
        public List<agrupa_grupo> agrupa_grupo { get; set; }
    }
    public class agrupa_subcuenta
    {
        public int subcuenta { get; set; }
        public string nom_subcuenta { get; set; }
        public decimal subcuenta_t_saldoini { get; set; }
        public decimal subcuenta_t_debito { get; set; }
        public decimal subcuenta_t_credito { get; set; }
        public decimal subcuenta_t_saldo { get; set; }
        public decimal subcuenta_t_saldoininiff { get; set; }
        public decimal subcuenta_t_debitoniff { get; set; }
        public decimal subcuenta_t_creditoniff { get; set; }
        public decimal subcuenta_t_saldoniff { get; set; }
        public List<detalleCuenta> detalleCuenta { get; set; }
        public List<agrupa_nit> agrupa_nit { get; set; }
        public List<listacentro> listacentro { get; set; }
    }
    public class agrupa_cuenta
    {
        public int cuenta { get; set; }
        public string nom_cuenta { get; set; }
        public decimal cuenta_t_saldoini { get; set; }
        public decimal cuenta_t_debito { get; set; }
        public decimal cuenta_t_credito { get; set; }
        public decimal cuenta_t_saldo { get; set; }
        public decimal cuenta_t_saldoininiff { get; set; }
        public decimal cuenta_t_debitoniff { get; set; }
        public decimal cuenta_t_creditoniff { get; set; }
        public decimal cuenta_t_saldoniff { get; set; }
        public List<agrupa_subcuenta> agrupa_subcuenta { get; set; }
    }
    public class agrupa_grupo
    {
        public int grupo { get; set; }
        public string nom_grupo { get; set; }
        public decimal grupo_t_saldoini { get; set; }
        public decimal grupo_t_debito { get; set; }
        public decimal grupo_t_credito { get; set; }
        public decimal grupo_t_saldo { get; set; }
        public decimal grupo_t_saldoininiff { get; set; }
        public decimal grupo_t_debitoniff { get; set; }
        public decimal grupo_t_creditoniff { get; set; }
        public decimal grupo_t_saldoniff { get; set; }
        public List<agrupa_cuenta> agrupa_cuenta { get; set; }
    }

    public class agrupa_nit
    {
        public int nit { get; set; }

        public string documento { get; set; }
        public string nombre_nit { get; set; }
        public decimal nit_t_saldoini { get; set; }
        public decimal nit_t_debito { get; set; }
        public decimal nit_t_credito { get; set; }
        public decimal nit_t_saldo { get; set; }
        public decimal nit_t_saldoininiff { get; set; }
        public decimal nit_t_debitoniff { get; set; }
        public decimal nit_t_creditoniff { get; set; }
        public decimal nit_t_saldoniff { get; set; }
        public List<detalleCuenta> detalleCuenta { get; set; }
    }

    public class seleccion_cuenta
    {
        public int id_cuenta { get; set; }
        public string numero_cuenta { get; set; }
        public string nombre_cuenta { get; set; }
        public decimal scuenta_t_saldoini { get; set; }
        public decimal scuenta_t_debito { get; set; }
        public decimal scuenta_t_credito { get; set; }
        public decimal scuenta_t_saldo { get; set; }
        public decimal scuenta_t_saldoininiff { get; set; }
        public decimal scuenta_t_debitoniff { get; set; }
        public decimal scuenta_t_creditoniff { get; set; }
        public decimal scuenta_t_saldoniff { get; set; }
        public List<listacentro> listacentro { get; set; }
    }

    public class listacentro
    {
        public int id_centro { get; set; }
        public string codigo_centro { get; set; }
        public string nombre_centro { get; set; }
        public decimal scentro_t_saldoini { get; set; }
        public decimal scentro_t_debito { get; set; }
        public decimal scentro_t_credito { get; set; }
        public decimal scentro_t_saldo { get; set; }
        public decimal scentro_t_saldoininiff { get; set; }
        public decimal scentro_t_debitoniff { get; set; }
        public decimal scentro_t_creditoniff { get; set; }
        public decimal scentro_t_saldoniff { get; set; }
        public List<listaclase> listaclase { get; set; }
        public List<detalleCuenta> detalleCuenta { get; set; }
        public List<agrupa_nit> agrupa_nit { get; set; }
        public List<lista_mov> lista_mov { get; set; }
    }

    public class listaclase
    {
        public int clase { get; set; }
        public string nom_clase { get; set; }
        public decimal Ctotalsaldoini { get; set; }
        public decimal Ctotaldebito { get; set; }
        public decimal Ctotalcredito { get; set; }
        public decimal Ctotalsaldo { get; set; }
        public decimal Ctotalsaldoininiff { get; set; }
        public decimal Ctotaldebitoniff { get; set; }
        public decimal Ctotalcreditoniff { get; set; }
        public decimal Ctotalsaldoniff { get; set; }
        public List<agrupa_grupo> agrupa_grupo { get; set; }
    }

    //public class ViewModelReportepdfmov
    //{
    //    public string titulo { get; set; }
    //    public DateTime fechaReporte { get; set; }
    //    public string fechaInicio { get; set; }
    //    public string fechafin { get; set; }
    //    public string cuentaIni { get; set; }
    //    public string cuentaFin { get; set; }
    //    public decimal Grantotaldebito { get; set; }
    //    public decimal Grantotalcredito { get; set; }
    //    public decimal Grantotalsaldo { get; set; }
    //    public decimal Grantotaldebitoniff { get; set; }
    //    public decimal Grantotalcreditoniff { get; set; }
    //    public decimal Grantotalsaldoniff { get; set; }
    //     public List<listaclasemov> listaclasemov { get; set; }
    //}

    //public class listaclasemov
    //{
    //    public int clase { get; set; }
    //    public string nom_clase { get; set; }
    //    public decimal Ctotaldebito { get; set; }
    //    public decimal Ctotalcredito { get; set; }
    //    public decimal Ctotalsaldo { get; set; }
    //    public decimal Ctotaldebitoniff { get; set; }
    //    public decimal Ctotalcreditoniff { get; set; }
    //    public decimal Ctotalsaldoniff { get; set; }
    //    public List<agrupa_grupomov> agrupa_grupomov { get; set; }
    //}
    //public class agrupa_grupomov
    //{
    //    public int grupo { get; set; }
    //    public string nom_grupo { get; set; }
    //    public decimal grupo_t_debito { get; set; }
    //    public decimal grupo_t_credito { get; set; }
    //    public decimal grupo_t_saldo { get; set; }
    //    public decimal grupo_t_debitoniff { get; set; }
    //    public decimal grupo_t_creditoniff { get; set; }
    //    public decimal grupo_t_saldoniff { get; set; }
    //    public List<agrupa_cuentamov> agrupa_cuentamov { get; set; }
    //}

    //public class agrupa_cuentamov
    //{
    //    public int cuenta { get; set; }
    //    public string nom_cuenta { get; set; }
    //    public decimal cuenta_t_debito { get; set; }
    //    public decimal cuenta_t_credito { get; set; }
    //    public decimal cuenta_t_saldo { get; set; }
    //    public decimal cuenta_t_debitoniff { get; set; }
    //    public decimal cuenta_t_creditoniff { get; set; }
    //    public decimal cuenta_t_saldoniff { get; set; }
    //    public List<agrupa_subcuentamov> agrupa_subcuentamov { get; set; }
    //}
    //public class agrupa_subcuentamov
    //{
    //    public int subcuenta { get; set; }
    //    public string nom_subcuenta { get; set; }
    //    public decimal subcuenta_t_debito { get; set; }
    //    public decimal subcuenta_t_credito { get; set; }
    //    public decimal subcuenta_t_saldo { get; set; }
    //    public decimal subcuenta_t_debitoniff { get; set; }
    //    public decimal subcuenta_t_creditoniff { get; set; }
    //    public decimal subcuenta_t_saldoniff { get; set; }
    //    public List<detalleCuentamov> detalleCuentamov { get; set; }

    //}
    //public class detalleCuentamov
    //{
    //    public Int32 id_cuenta_det { get; set; }
    //    public string numero_cuenta_det { get; set; }
    //    public string nombre_cuenta_det { get; set; }

    //    public decimal debito_det { get; set; }
    //    public decimal credito_det { get; set; }
    //    public decimal saldo_det { get; set; }
    //    public decimal debitoniff_det { get; set; }
    //    public decimal creditoniff_det { get; set; }
    //    public decimal saldoniff_det { get; set; }
    //    public List<listacentromov> listacentromov { get; set; }

    //}
    //public class listacentromov
    //{
    //    public int id_centro { get; set; }
    //    public string codigo_centro { get; set; }
    //    public string nombre_centro { get; set; }
    //    public decimal scentro_t_debito { get; set; }
    //    public decimal scentro_t_credito { get; set; }
    //    public decimal scentro_t_saldo { get; set; }
    //    public decimal scentro_t_debitoniff { get; set; }
    //    public decimal scentro_t_creditoniff { get; set; }
    //    public decimal scentro_t_saldoniff { get; set; }
    //    public List<lista_mov> lista_mov { get; set; }
    //}
    //public class lista_mov
    //{
    //    public long doc { get; set; }
    //    public string prefijo { get; set; }
    //    public string nom_docuento { get; set; }
    //    public decimal debito_mov { get; set; }
    //    public decimal credito_mov { get; set; }
    //    public decimal saldo_mov { get; set; }

    //    public decimal debitoniff_mov { get; set; }
    //    public decimal creditoniff_mov { get; set; }
    //    public decimal saldoniff_mov { get; set; }
    //}
}


