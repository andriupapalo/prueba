using System;

namespace Homer_MVC.IcebergModel
{
    public class rseparacionmercanciaModel
    {
        public int id { get; set; }
        public int bodega { get; set; }
        public string codigo { get; set; }
        public int? cliente { get; set; }
        public DateTime fecha { get; set; }
        public int cantidad { get; set; }
        public int? idordentaller { get; set; }
        public int? idcita { get; set; }
        public int? idpedido { get; set; }
        public string notas { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string placa { get; set; }
        public string color { get; set; }
        public string fechaFinal { get; set; }
    }
}