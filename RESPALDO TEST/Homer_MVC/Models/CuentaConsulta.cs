using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class CuentaConsulta
    {

        public int ano { get; set; }
        public int mes { get; set; }
        public int cuentaId { get; set; }
        public string centroPrefijo { get; set; }
        public string cuenta { get; set; }
        public int centroId { get; set; }
        public string cuentaDescripcion { get; set; }
        public string centro { get; set; }
        public string prefijoDocumento { get; set; }
        public string nombreDocumento { get; set; }
        public int nit { get; set; }
        public decimal saldo_ini { get; set; }
        public decimal saldo_inicial_niff { get; set; }
        public decimal debito { get; set; }
        public decimal credito { get; set; }
        public decimal debitoNiff { get; set; }
        public decimal creditoNiff { get; set; }
        public decimal total { get; set; }
        public decimal totalNiff { get; set; }
        public string terceroNombre { get; set; }
        public string terceroDocumento { get; set; }
        public bool visible { get; set; }
        public bool esAfectable { get; set; }
        public int terceroId { get; set; }
        public List<CuentaConsulta> cuentasHijas;
        public List<CuentaConsulta> centrosHijos;
        public List<CuentaConsulta> NitsHijos;
        public List<CuentaConsulta> MovimientosHijos;

        public CuentaConsulta()
        {
            cuentasHijas = new List<CuentaConsulta>();
            centrosHijos = new List<CuentaConsulta>();
            NitsHijos = new List<CuentaConsulta>();
            MovimientosHijos = new List<CuentaConsulta>();
            visible = true;
        }
        //public decimal saldo_ininiff { get; set; }
        //public decimal debitoniff { get; set; }
        //public decimal creditoniff { get; set; }

    }
}