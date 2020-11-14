using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class CsCalcularCostoPromedio
    {
        private CultureInfo Cultura = new CultureInfo("is-IS");

        private readonly Iceberg_Context context = new Iceberg_Context();

        public decimal calcularCostoPromedio(decimal cantidad_recibir, decimal valor_unitario, string codigo, int bodega)
        {


            decimal nuevo_costo_promedio = 0;
            var kardex2 = (from x in context.vw_kardex2
                           where x.codigo == codigo && x.bodega == bodega
                           select new
                           {
                               x.stock,
                               x.costoProm,
                               x.ano,
                               x.mes
                           }).OrderByDescending(m => new { m.ano, m.mes }).FirstOrDefault();





            if (kardex2 != null)
            {
                if (kardex2.stock != 0)
                {
                    nuevo_costo_promedio = Math.Round(Convert.ToDecimal(
                        (kardex2.stock * kardex2.costoProm + cantidad_recibir * valor_unitario) /
                        (kardex2.stock + cantidad_recibir), Cultura));
                }
            }
            else
            {
                nuevo_costo_promedio = Math.Round(valor_unitario);
            }

           

            return nuevo_costo_promedio;
        }




    }
}