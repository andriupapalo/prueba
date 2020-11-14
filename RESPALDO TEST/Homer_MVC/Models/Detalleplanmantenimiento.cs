using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class Detalleplanmantenimiento
    {
        public int id { get; set; }
        public int modgeneral { get; set; }
        public string Descripcion { get; set; }
        public string referencia { get; set; }
        public string ref_descripcion { get; set; }
        public string tempario { get; set; }
        public string operacion { get; set; }
        public decimal? cantidad { get; set; }
        public int? inspeccion { get; set; }
        public string listaprecios { get; set; }
        public string tipo { get; set; }
        public decimal? mano_obra { get; set; }
        public decimal? insumos { get; set; }
        public string tiempo_insumos { get; set; }
        public decimal? tiempo_insumos2 { get; set; }
        public decimal? valor_total { get; set; }

    }

    public class listainsumos
    {
        [Required]
        public int? modelogeneral { get; set; }
        [Required]
        public string insumo1 { get; set; }
        [Required]
        public string insumo2 { get; set; }
        [Required]
        public string insumo3 { get; set; }
        [Required]
        public string insumo4 { get; set; }
        [Required]
        public string insumo5 { get; set; }
        [Required]
        public string insumo6 { get; set; }
        [Required]
        public string insumo7 { get; set; }
        [Required]
        public string insumo8 { get; set; }
        [Required]
        public string insumo9 { get; set; }
        [Required]
        public string insumo10 { get; set; }
        [Required]
        public string insumo11 { get; set; }
        [Required]
        public string insumo12 { get; set; }
        [Required]
        public string insumo13 { get; set; }
        [Required]
        public string insumo14 { get; set; }
        [Required]
        public string insumo15 { get; set; }
        [Required]
        public string insumo16 { get; set; }
        [Required]
        public string insumo17 { get; set; }
        [Required]
        public string insumo18 { get; set; }
        [Required]
        public string insumo19 { get; set; }
        [Required]
        public string insumo20 { get; set; }

    }

    public class insumosporplan
    {
        public int idplan { get; set; }
        public decimal porcentaje { get; set; }
        public decimal precio { get; set; }
        public string precio2 { get; set; }
    }

    public class manoobraporplan
    {
        public int idplan { get; set; }
        public decimal horas { get; set; }
        public decimal precio { get; set; }
        public string precio2 { get; set; }
    }
    public class totalporplan
    {
        public int idplan { get; set; }
        public decimal precio { get; set; }
        public string precio2 { get; set; }
    }

    public class detalleoperacionesPlan
    {
        public string codigo { get; set; }
        public string operacion { get; set; }
        public decimal tiempo { get; set; }
        public decimal preciomatriz { get; set; }
        public string preciomatriz2 { get; set; }
        public string iva { get; set; }
        public decimal porcentajeiva { get; set; }
        public decimal preciototal { get; set; }
    }
}