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
    
    public partial class vw_Horarios_Asesores_Planta
    {
        public int horario_id { get; set; }
        public string tipo_horario { get; set; }
        public Nullable<int> usuario_id { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public bool turno { get; set; }
        public bool checks { get; set; }
        public string user_nombre { get; set; }
        public string user_apellido { get; set; }
        public string nombreasesorcompleto { get; set; }
        public string lunes_total { get; set; }
        public string martes_total { get; set; }
        public string miercoles_total { get; set; }
        public string jueves_total { get; set; }
        public string viernes_total { get; set; }
        public string sabado_total { get; set; }
        public string domingo_total { get; set; }
    }
}
