//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Homer_MVC.IcebergModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Movimiento
    {
        public int Id { get; set; }
        public Nullable<int> TiendaId { get; set; }
        public string EmpleadoId { get; set; }
        public Nullable<System.DateTime> FechaMovimiento { get; set; }
        public Nullable<int> Horaini { get; set; }
        public Nullable<int> Horafin { get; set; }
        public double TotalHoras { get; set; }
    
        public virtual Empleado Empleado { get; set; }
        public virtual Tienda Tienda { get; set; }
    }
}
