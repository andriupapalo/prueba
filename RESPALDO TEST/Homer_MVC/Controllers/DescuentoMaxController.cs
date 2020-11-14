using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class DescuentoMaxController : Controller
    {

        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: DescuentoMax
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(formdescmax descuento )
            {
            tmaximodescuentomo maxdescuento = new tmaximodescuentomo();

            maxdescuento.descripcion = descuento.descripcion;
            maxdescuento.estado = descuento.estado;
            maxdescuento.fec_creacion = DateTime.Now;
            maxdescuento.valor = descuento.valor;
            maxdescuento.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            context.tmaximodescuentomo.Add(maxdescuento);
            context.SaveChanges();
            TempData["mensaje"] = "Descuento creado con exito";
            return View();
            }

        public JsonResult VermMaximoDescuento() {
            var datos = context.tmaximodescuentomo.Select(x => new
                {
                x.descripcion,
                estado = x.estado != false ? "Activo" : "inactivo",
                x.id,
                x.valor
                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public ActionResult Edit(int id) {
            tmaximodescuentomo descuento = context.tmaximodescuentomo.Where(x => x.id == id).FirstOrDefault();
            formdescmax modelo = new formdescmax();
            modelo.id = descuento.id;
            modelo.descripcion = descuento.descripcion;
            modelo.estado = Convert.ToBoolean(descuento.estado);
            modelo.valor = Convert.ToDecimal(descuento.valor, miCultura);
            return View(modelo);
            }
        [HttpPost]
        public ActionResult Edit(formdescmax modelo)
            {
            tmaximodescuentomo descuento = context.tmaximodescuentomo.Where(x => x.id == modelo.id).FirstOrDefault();
            descuento.descripcion = modelo.descripcion;
            descuento.estado = modelo.estado;
            descuento.valor = Convert.ToDecimal(modelo.valor, miCultura);
            context.Entry(descuento).State = EntityState.Modified;
            context.SaveChanges();
            TempData["mensaje"] = "Descuento actualizado con exito";
            return View(modelo);
            }

            }
    }