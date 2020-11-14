using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class modificarCosto
    {
        public List<referencias> referencias { get; set; }
        public string tipoEntrada { get; set; }
        public long numeroRegistro { get; set; }
        public string fechaRegistro { get; set; }
        public string nombreBodega { get; set; }
        public int secuencia { get; set; }
        public string numeroRefencia { get; set; }
        public string descripcion { get; set; }
        public string notas { get; set; }
        public decimal TotalTotales { get; set; }


    }
    public class referencias
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal costoModificacionSubida { get; set; }
        public decimal costoModificacionBajada { get; set; }
        public int secuencia { get; set; }
        public decimal total { get; set; }




    }


}