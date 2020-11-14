using System;
using System.Collections.Generic;

namespace Homer_MVC.ViewModels.contabpdf
{
    public class ViewModelReportepdfmov
    {
        public string titulo { get; set; }
        public DateTime fechaReporte { get; set; }
        public string fechaInicio { get; set; }
        public string fechafin { get; set; }
        public string cuentaIni { get; set; }
        public string cuentaFin { get; set; }
        public decimal Grantotaldebito { get; set; }
        public decimal Grantotalcredito { get; set; }
        public decimal Grantotalsaldo { get; set; }
        public decimal Grantotaldebitoniff { get; set; }
        public decimal Grantotalcreditoniff { get; set; }
        public decimal Grantotalsaldoniff { get; set; }
        public List<listaclasemov> listaclasemov { get; set; }
    }

    public class listaclasemov
    {
        public int clase { get; set; }
        public string nom_clase { get; set; }
        public decimal Ctotaldebito { get; set; }
        public decimal Ctotalcredito { get; set; }
        public decimal Ctotalsaldo { get; set; }
        public decimal Ctotaldebitoniff { get; set; }
        public decimal Ctotalcreditoniff { get; set; }
        public decimal Ctotalsaldoniff { get; set; }
        public List<agrupa_grupomov> agrupa_grupomov { get; set; }
    }
    public class agrupa_grupomov
    {
        public int grupo { get; set; }
        public string nom_grupo { get; set; }
        public decimal grupo_t_debito { get; set; }
        public decimal grupo_t_credito { get; set; }
        public decimal grupo_t_saldo { get; set; }
        public decimal grupo_t_debitoniff { get; set; }
        public decimal grupo_t_creditoniff { get; set; }
        public decimal grupo_t_saldoniff { get; set; }
        public List<agrupa_cuentamov> agrupa_cuentamov { get; set; }
    }

    public class agrupa_cuentamov
    {
        public int cuenta { get; set; }
        public string nom_cuenta { get; set; }
        public decimal cuenta_t_debito { get; set; }
        public decimal cuenta_t_credito { get; set; }
        public decimal cuenta_t_saldo { get; set; }
        public decimal cuenta_t_debitoniff { get; set; }
        public decimal cuenta_t_creditoniff { get; set; }
        public decimal cuenta_t_saldoniff { get; set; }
        public List<agrupa_subcuentamov> agrupa_subcuentamov { get; set; }
    }
    public class agrupa_subcuentamov
    {
        public int subcuenta { get; set; }
        public string nom_subcuenta { get; set; }
        public decimal subcuenta_t_debito { get; set; }
        public decimal subcuenta_t_credito { get; set; }
        public decimal subcuenta_t_saldo { get; set; }
        public decimal subcuenta_t_debitoniff { get; set; }
        public decimal subcuenta_t_creditoniff { get; set; }
        public decimal subcuenta_t_saldoniff { get; set; }
        public List<detalleCuentamov> detalleCuentamov { get; set; }

    }
    public class detalleCuentamov
    {
        public int id_cuenta_det { get; set; }
        public string numero_cuenta_det { get; set; }
        public string nombre_cuenta_det { get; set; }

        public decimal debito_det { get; set; }
        public decimal credito_det { get; set; }
        public decimal saldo_det { get; set; }
        public decimal debitoniff_det { get; set; }
        public decimal creditoniff_det { get; set; }
        public decimal saldoniff_det { get; set; }
        public List<listacentromov> listacentromov { get; set; }
        public List<lista_mov> lista_mov { get; set; }
        public List<agrupa_nit_mov> agrupa_nit_mov { get; set; }

    }
    public class listacentromov
    {
        public int id_centro { get; set; }
        public string codigo_centro { get; set; }
        public string nombre_centro { get; set; }
        public decimal scentro_t_debito { get; set; }
        public decimal scentro_t_credito { get; set; }
        public decimal scentro_t_saldo { get; set; }
        public decimal scentro_t_debitoniff { get; set; }
        public decimal scentro_t_creditoniff { get; set; }
        public decimal scentro_t_saldoniff { get; set; }
        public List<lista_mov> lista_mov { get; set; }
        public List<agrupa_nit_mov> agrupa_nit_mov { get; set; }
    }
    public class lista_mov
    {
        public long doc { get; set; }
        public string prefijo { get; set; }
        public string nom_docuento { get; set; }
        public decimal debito_mov { get; set; }
        public decimal credito_mov { get; set; }
        public decimal saldo_mov { get; set; }

        public decimal debitoniff_mov { get; set; }
        public decimal creditoniff_mov { get; set; }
        public decimal saldoniff_mov { get; set; }
    }

    public class agrupa_nit_mov
    {
        public int nit { get; set; }

        public string documento { get; set; }
        public string nombre_nit { get; set; }
        public decimal nit_t_debito { get; set; }
        public decimal nit_t_credito { get; set; }
        public decimal nit_t_saldo { get; set; }
        public decimal nit_t_debitoniff { get; set; }
        public decimal nit_t_creditoniff { get; set; }
        public decimal nit_t_saldoniff { get; set; }
        public List<lista_mov> lista_mov { get; set; }

    }
}