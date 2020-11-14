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
    public class InsumosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: tempario


        // GET: Insumos
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormInsumos modelo)
            {
            Tinsumo insumo = new Tinsumo();
            insumo.descripcion = modelo.descripcion;
            insumo.codigo = modelo.codigo;
            insumo.horas_insumo = modelo.horas_insumo;
            insumo.porcentaje = modelo.porcentaje;
            context.Tinsumo.Add(insumo);
            context.SaveChanges();
            TempData["mensaje"] = "Insumo creado con exito";


            return View();
            }

        public JsonResult BuscarInsumos() {


            var datos = context.Tinsumo.Select(x => new
                {
                x.id,
                x.codigo,
                x.descripcion,
                x.horas_insumo,
                x.porcentaje
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public ActionResult Edit(int id)
            {
            Tinsumo insumo = context.Tinsumo.Where(x => x.id == id).FirstOrDefault();
            FormInsumos modelo = new FormInsumos();
            modelo.id = insumo.id;
            modelo.descripcion = insumo.descripcion;
            modelo.codigo = insumo.codigo;
            modelo.horas_insumo = Convert.ToDecimal(insumo.horas_insumo, miCultura);
            modelo.porcentaje = Convert.ToDecimal(insumo.porcentaje, miCultura);
            modelo.estado = Convert.ToBoolean(insumo.estado);
            modelo.razoninactivo = insumo.razoninactivo;


            return View(modelo);
            }
        [HttpPost]
        public ActionResult Edit(FormInsumos modelo)
            {
            Tinsumo insumo = context.Tinsumo.Where(x => x.id == modelo.id).FirstOrDefault();              
            insumo.descripcion = modelo.descripcion;
            insumo.codigo = modelo.codigo;
            insumo.horas_insumo = modelo.horas_insumo;
            insumo.porcentaje = modelo.porcentaje;
            insumo.estado = modelo.estado;
            insumo.razoninactivo = modelo.razoninactivo;
            context.Entry(insumo).State = EntityState.Modified;
            context.SaveChanges();
            TempData["mensaje"] = "Insumo actualizado con exito";
            return View(modelo);
            }


        }
        }