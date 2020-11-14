using Homer_MVC.IcebergModel;
using Homer_MVC.Models.SeguimientoModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers.Seguimiento
{
    public class SeguimientosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        // GET: Seguimientos
        public ActionResult Seguimiento()
        {
            var modelo = new Seguimientos();
            ViewBag.Modulo = new SelectList(context.ModuloSeguimientos.Where(x => x.Estado != false).ToList(), "Id", "Modulo");
            var modelo2 = new ModuloSeguimientos();
            return View(modelo);
        }
        public ActionResult CreateModuloSeguimiento()
        {
            return View();
        }

        #region accion modulo Seguimiento
        public JsonResult GuardarModuloSeguimiento(ModuloSeguimientosModel modulo)
        {
            var OK = false;
            var mensaje = "error";
            var guardar = 0;

            if (ModelState.IsValid)
            {
                var existe = context.ModuloSeguimientos.Where(x => x.Codigo == modulo.Codigo).Count();
                if (existe >= 1)
                {
                    mensaje = "Registro Ya Existe! El 'Codigo' no puede estar Duplicado";
                }
                else {

                    var modulox = new ModuloSeguimientos
                    {
                        Codigo = modulo.Codigo.Value,
                        Modulo = modulo.Modulo,
                        Estado = modulo.Estado,
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    };
                    context.ModuloSeguimientos.Add(modulox);
                    guardar = context.SaveChanges();
                }
            }
            else
            {
                mensaje = "Faltan Campos esenciales";
            }
            if (guardar > 0)
            {
                OK = true;
                mensaje = "Registro Almacenado";
            }
            var data = new { OK, mensaje };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LisModuloSeguimiento()
        {

            var data = context.ModuloSeguimientos.Select(d => new {
                d.Id,
                d.Modulo,
                d.Estado,
                d.Codigo
            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EliminarModuloSeguimiento(int Id)
        {

            ModuloSeguimientos buscarModuloSeguimientos = context.ModuloSeguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarModuloSeguimientos != null)
            {
                context.Entry(buscarModuloSeguimientos).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EstadoModuloSeguimiento(int Id)
        {

            ModuloSeguimientos buscarModuloSeguimientos = context.ModuloSeguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarModuloSeguimientos != null)
            {
                if (buscarModuloSeguimientos.Estado == true)
                {
                    buscarModuloSeguimientos.Estado = false;
                }
                else
                {
                    buscarModuloSeguimientos.Estado = true;
                }

                // context.Entry(buscarModuloSeguimientos.Estado=true).State = EntityState.Modified;

                int Actualiza = context.SaveChanges();
                if (Actualiza > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion modulosSeguimiento

        #region accion Seguimiento

        public JsonResult GuardarSeguimiento(SeguimientosModel Seguimiento)
        {
            var OK = false;
            var mensaje = "error";
            var guardar = 0;

            if (ModelState.IsValid)
            {
                var existe = context.Seguimientos.Where(x => x.Codigo == Seguimiento.Codigo).Count();
                if (existe >= 1)
                {
                    mensaje = "Registro Ya Existe! El 'Codigo' no puede estar Duplicado";
                }
                else
                {
                    context.Seguimientos.Add(new Seguimientos
                    {
                        Codigo = Seguimiento.Codigo.Value,
                        Modulo = Seguimiento.Modulo.Value,
                        EsManual = Seguimiento.EsManual,
                        EsEstandar = Seguimiento.EsEstandar,
                        Evento = Seguimiento.Evento,
                        Estado = Seguimiento.Estado,
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    });
                    guardar = context.SaveChanges();
                }
            }
            else
            {
                mensaje = "Faltan Campos esenciales";
            }
            if (guardar > 0)
            {
                OK = true;
                mensaje = "Registro Almacenado";
            }
            var data = new { OK, mensaje };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LisSeguimiento()
        {

            var data = context.Seguimientos.Select(d => new {
                d.Id,
                d.Codigo,
                d.EsManual,
                d.EsEstandar,
                d.Evento,
                d.Estado,
                Modulo = d.Modulo + " * " + d.ModuloSeguimientos.Modulo,

            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EliminarSeguimiento(int Id)
        {
            Seguimientos buscarSeguimientos = context.Seguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarSeguimientos != null)
            {
                context.Entry(buscarSeguimientos).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EstadoSeguimiento(int Id)
        {
            Seguimientos buscarSeguimientos = context.Seguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarSeguimientos != null)
            {
                if (buscarSeguimientos.Estado == true)
                {
                    buscarSeguimientos.Estado = false;
                }
                else
                {
                    buscarSeguimientos.Estado = true;
                }

                int Actualiza = context.SaveChanges();
                if (Actualiza > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion modulosSeguimiento

        public ActionResult MotivoSeguimiento()
        {
            var modelo = new SeguimientoAnulacion();
            ViewBag.ModuloSSeguimiento = new SelectList(context.ModuloSeguimientos.Where(x => x.Estado != false).ToList(), "Codigo", "Modulo");
            ViewBag.CodigoSeguimiento = new SelectList(context.Seguimientos.Where(x => x.Estado != false && x.EsManual == true).ToList(), "Id", "Evento");
            ViewBag.IdMotivoAnulacion = new SelectList(context.motivoanulacion.Where(x => x.estado != false).ToList(), "id", "motivo");
            return View(modelo);
        }

        public JsonResult GuardarSeguimientoAnulacion(SeguimientoAnulacion seguimientoAnulacion)
        {
            var OK = false;
            var mensaje = "Se ha Detectado un error !";
            var guardar = 0;

            if (ModelState.IsValid)
            {
                var modulox = new MotivoSeguimientos
                {
                    Estado = seguimientoAnulacion.EstadoAnulacion,
                    CodigoSeguimiento = seguimientoAnulacion.CodigoSeguimiento.Value,
                    IdMotivoAnulacion = seguimientoAnulacion.IdMotivoAnulacion.Value,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };
                context.MotivoSeguimientos.Add(modulox);
                guardar = context.SaveChanges();
            }
            else
            {
                mensaje = "Faltan Campos Obligatorios!";
            }
            if (guardar > 0)
            {
                OK = true;
                mensaje = "Registro Almacenado";
            }
            var data = new { OK, mensaje };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DropListSeguimientosModulo(int Codigo)
        {
            var CodigoSeguimiento = context.Seguimientos.Where(x => x.Estado != false && x.EsManual == true && x.ModuloSeguimientos.Codigo == Codigo).Select(x => new {
                x.Id,
                x.Evento,
            }).ToList();

            return Json(CodigoSeguimiento, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LisMotivoAnulacionSeguimiento()
        {
            var data = context.MotivoSeguimientos.Select(d => new {
                d.Id,
                CodigoSeguimiento = d.CodigoSeguimiento + " * " + d.Seguimientos.Evento, //CodigoSeguimiento =IdSeguimiento
                IdMotivoAnulacion = d.IdMotivoAnulacion + " * " + d.motivoanulacion.motivo,
                d.Estado,
            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult EliminarSeguimientoMotivoAnulacion(int Id)
        {
            MotivoSeguimientos buscarSeguimientoMotivo = context.MotivoSeguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarSeguimientoMotivo != null)
            {
                context.Entry(buscarSeguimientoMotivo).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EstadoSeguimientoMotivoAnulacion(int Id)
        {
            MotivoSeguimientos buscarSeguimientoMotivo = context.MotivoSeguimientos.FirstOrDefault(x => x.Id == Id);
            if (buscarSeguimientoMotivo != null)
            {
                if (buscarSeguimientoMotivo.Estado == true)
                {
                    buscarSeguimientoMotivo.Estado = false;
                }
                else
                {
                    buscarSeguimientoMotivo.Estado = true;
                }

                int Actualiza = context.SaveChanges();
                if (Actualiza > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

    }
}