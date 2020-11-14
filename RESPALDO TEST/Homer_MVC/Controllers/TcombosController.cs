using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class TcombosController : Controller
    {

        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: Tcombos
        public ActionResult Index()
        {
            ViewBag.Operacionescombo =
          new SelectList(context.ttempario.Select(x => new { Descripcion= x.codigo+" | "+x.operacion, x.codigo }), "codigo", "Descripcion");

            return View();
        }

        public JsonResult BuscaroperacionesDatos(string operacion) {


            var datos = context.ttempario.Where(x => x.codigo == operacion).Select(d => new { d.codigo, d.operacion, d.HoraCliente, d.HoraOperario }).FirstOrDefault();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        [HttpPost]
        public ActionResult Index(formCombos modelo)
            {
            ViewBag.Operacionescombo =
          new SelectList(context.ttempario.Select(x => new { Descripcion = x.codigo + " | " + x.operacion, x.codigo }), "codigo", "Descripcion");

            int Cantidadlineas = Convert.ToInt32(Request["toperaciones"]);
            if (Cantidadlineas > 0)
                {

                tcombos combos = new tcombos();
                combos.descripcion = modelo.descripcion;
                combos.estado = modelo.estado;
                combos.razoninactivo = modelo.razoninactivo;
                combos.fechacreacion = DateTime.Now;
                combos.usercreacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.tcombos.Add(combos);
                context.SaveChanges();
                for (int i = 0; i < Cantidadlineas; i++)
                    {
                    if (Request["operacombo"+i] !=null)
                        {
                        tcombodetalle detalle = new tcombodetalle();
                        detalle.idtcombo = combos.id;
                        detalle.tempario = Request["operacombo" + i];
                        detalle.Estado = true;
                        context.tcombodetalle.Add(detalle);
                        context.SaveChanges();
                        }


                    }
                TempData["mensaje"] = "Combo creado con exito";
                }
            else {
                TempData["mensaje_error"] = "El combo a registrar debe tener al menos una operacion registrada";
                }


            return View();
            }

        public JsonResult BuscarCombos() {
            var datos = context.tcombos.Select(x => new
                {
                x.id,
                x.descripcion,
                Estado = x.estado != false ? "Activo" : "Inactivo",
                operacion = context.tcombodetalle.Where(s => s.idtcombo == x.id && s.Estado==true).Select(f => f.ttempario.operacion).ToList()
                }).ToList();




            return Json(datos, JsonRequestBehavior.AllowGet);
            }



        public ActionResult Edit(int id) {

            ViewBag.Operacionescombo =
        new SelectList(context.ttempario.Select(x => new { Descripcion = x.codigo + " | " + x.operacion, x.codigo }), "codigo", "Descripcion");

            var combo = context.tcombos.Where(x => x.id == id).Select(d => new { d.id, d.razoninactivo, d.estado, d.descripcion }).FirstOrDefault();

            formCombos modelo = new formCombos();
            modelo.id = combo.id;
            modelo.descripcion = combo.descripcion;
            modelo.estado = Convert.ToBoolean(combo.estado);
            modelo.razoninactivo = combo.razoninactivo;

            return View(modelo);

            }


        public JsonResult BuscaroperacionesCarga(int  id)
            {
            var datos = context.tcombodetalle.Where(x => x.idtcombo == id && x.Estado==true).Select(d => new { codigo = d.idtcombo,
                d.ttempario.operacion,
                HoraCliente = d.ttempario.HoraCliente!= null? d.ttempario.HoraCliente:"0" ,
                HoraOperario = d.ttempario.HoraOperario != null ? d.ttempario.HoraOperario:0 ,
                d.id }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public JsonResult AgregarOperacioonEditar(int id, string operacion)
            {
            tcombodetalle detalle = new tcombodetalle();
            detalle.idtcombo = id;
            detalle.tempario = operacion;
            detalle.Estado = true;
            context.tcombodetalle.Add(detalle);
            context.SaveChanges();


            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Eliminaroperacioneditar(int id)
            {
            tcombodetalle detalle = context.tcombodetalle.Where(x => x.id == id).FirstOrDefault();
     
            detalle.Estado = false;
            context.Entry(detalle).State= EntityState.Modified;
            context.SaveChanges();


            return Json(true, JsonRequestBehavior.AllowGet);
            }



        }
    }