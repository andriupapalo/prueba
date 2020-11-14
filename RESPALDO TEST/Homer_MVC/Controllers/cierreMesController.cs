using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cierreMesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: cierreMes
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CerrarMesCandado(int ano, int mes)
        {
            meses_cierre buscarSiExiste = context.meses_cierre.FirstOrDefault(x => x.ano == ano && x.mes == mes);
            if (buscarSiExiste == null)
            {
                context.meses_cierre.Add(new meses_cierre
                {
                    ano = ano,
                    mes = mes,
                    fecha_realizacion = DateTime.Now,
                    usuario_creacion = Convert.ToInt32(Session["user_usuarioid"])
                });
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    return Json(new { mensaje = "El mes se ha cerrado correctamente" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { mensaje = "El mes ya se encontraba cerrado" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { mensaje = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AbrirMesCandado(int ano, int mes)
        {
            meses_cierre buscarSiExiste = context.meses_cierre.FirstOrDefault(x => x.ano == ano && x.mes == mes);
            if (buscarSiExiste != null)
            {
                context.Entry(buscarSiExiste).State = EntityState.Deleted;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    return Json(new { mensaje = "El mes se ha abierto correctamente" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { mensaje = "El mes se encuentra abierto" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { mensaje = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCierresDeMes()
        {
            var buscarCierres = (from cierres in context.meses_cierre
                                 join user in context.users
                                     on cierres.usuario_creacion equals user.user_id
                                 select new
                                 {
                                     cierres.ano,
                                     cierres.mes,
                                     cierres.fecha_realizacion,
                                     user_nombre = user.user_nombre + " " + user.user_apellido
                                 }).OrderByDescending(x => x.ano).ThenByDescending(x => x.mes).ToList();

            var cierresAux = buscarCierres.Select(x => new
            {
                x.ano,
                x.mes,
                fecha_realizacion = x.fecha_realizacion.ToShortDateString() + " " +
                                    x.fecha_realizacion.ToShortTimeString(),
                x.user_nombre
            });

            return Json(cierresAux, JsonRequestBehavior.AllowGet);
        }
    }
}