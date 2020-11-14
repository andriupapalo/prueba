using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class contabilidadAuxiliar
    {
        // Cabecera Auxiliar
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
        public List<Bodegas01> BodegasA01 { get; set; }

        //  public List<mov_cuentaA01> mov_cuentaA01 { get; set; }
    }

    public class Bodegas01
    {
        public int Id_Bodega { get; set; }
        public string cod_bodega { get; set; }
        public string nom_bodega { get; set; }
        public decimal Btotaldebito { get; set; }
        public decimal Btotalcredito { get; set; }
        public decimal Btotalsaldo { get; set; }
        public decimal Btotaldebitoniff { get; set; }
        public decimal Btotalcreditoniff { get; set; }
        public decimal Btotalsaldoniff { get; set; }
        // public List<Tercero01> TerceroA01 { get; set; }
        public List<Cuenta01> CuentaA01 { get; set; }

    }

    public class Cuenta01
    {
        public int Id_cuenta { get; set; }
        public string cod_cuenta { get; set; }
        public string nom_cuenta { get; set; }
        public decimal Ctotaldebito { get; set; }
        public decimal Ctotalcredito { get; set; }
        public decimal Ctotalsaldo { get; set; }
        public decimal Ctotaldebitoniff { get; set; }
        public decimal Ctotalcreditoniff { get; set; }
        public decimal Ctotalsaldoniff { get; set; }
        //public List<Documentos01> DocumentosA01 { get; set; }
        // public List<CentroCosto01> CentroCostoA01 { get; set; }
        public List<Tercero01> TerceroA01 { get; set; }
    }
    public class Tercero01
    {
        public int Id_tercero { get; set; }
        public string doc_tercero { get; set; }
        public string nom_tercero { get; set; }
        public decimal Tertotaldebito { get; set; }
        public decimal Tertotalcredito { get; set; }
        public decimal Tertotalsaldo { get; set; }
        public decimal Tertotaldebitoniff { get; set; }
        public decimal Tertotalcreditoniff { get; set; }
        public decimal Tertotalsaldoniff { get; set; }
        // public List<Cuenta01> CuentaA01 { get; set; }
        public List<CentroCosto01> CentroCostoA01 { get; set; }
    }
    //public class Cuenta01
    //{
    //    public int Id_cuenta { get; set; }
    //    public string cod_cuenta { get; set; }
    //    public string nom_cuenta { get; set; }
    //    public decimal Ctotaldebito { get; set; }
    //    public decimal Ctotalcredito { get; set; }
    //    public decimal Ctotalsaldo { get; set; }
    //    public decimal Ctotaldebitoniff { get; set; }
    //    public decimal Ctotalcreditoniff { get; set; }
    //    public decimal Ctotalsaldoniff { get; set; }
    //    //public List<Documentos01> DocumentosA01 { get; set; }
    //    public List<CentroCosto01> CentroCostoA01 { get; set; }
    //}

    public class CentroCosto01
    {
        public int Id_Centro { get; set; }
        public string pref_Centro { get; set; }
        public string nom_Centro { get; set; }
        public decimal CCtotaldebito { get; set; }
        public decimal CCtotalcredito { get; set; }
        public decimal CCtotalsaldo { get; set; }
        public decimal CCtotaldebitoniff { get; set; }
        public decimal CCtotalcreditoniff { get; set; }
        public decimal CCtotalsaldoniff { get; set; }

        public List<Documentos01> DocumentosA01 { get; set; }
    }
    public class Documentos01
    {
        public int Id_documento { get; set; }
        public string cod_documento { get; set; }
        public string nom_documento { get; set; }
        public string fechaDocumento { get; set; }
        public decimal Dtotaldebito { get; set; }
        public decimal Dtotalcredito { get; set; }
        public decimal Dtotalsaldo { get; set; }
        public decimal Dtotaldebitoniff { get; set; }
        public decimal Dtotalcreditoniff { get; set; }
        public decimal Dtotalsaldoniff { get; set; }

    }
}