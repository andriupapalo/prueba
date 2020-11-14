using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class CsCalcularMargenUtilidad
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public decimal MargenUtilidad(int bodega, string codigoref, int cantidad, decimal valorTotal, decimal valoriva)
        {
            decimal margenUtilidad = 0;
            decimal precioventa = 0;

            var costoprom = context.referencias_inven.Where(x => x.bodega == bodega && x.codigo == codigoref).OrderByDescending(x => new { x.ano, x.mes }).FirstOrDefault();

            precioventa = valorTotal - valoriva;

            if (costoprom != null && precioventa > 0)
            {

                decimal costot = costoprom.costo_prom * cantidad;
                precioventa = precioventa * cantidad;
                margenUtilidad = (precioventa - costot) / precioventa;



            }


            return margenUtilidad;

        }
    }
}