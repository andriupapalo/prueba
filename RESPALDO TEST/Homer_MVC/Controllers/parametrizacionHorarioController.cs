using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class parametrizacionHorarioController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        private static Expression<Func<vw_Horarios_Asesores_Planta, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_Horarios_Asesores_Planta), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_Horarios_Asesores_Planta, string>> lambda = Expression.Lambda<Func<vw_Horarios_Asesores_Planta, string>>(menuProperty, menu);

            return lambda;
        }
        // GET: parametrizacionHorario/Create
        public ActionResult Create(string tphorario, int? val, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                ViewBag.usuario_id = db.users.ToList();
                ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", val);

                if (Session["user_rolid"].ToString() == "4" || Session["user_rolid"].ToString() == "3")
                {
                    //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", usuario_actual);
                    //ViewBag.usuario_id = db.users.Where(x=> x.user_id == usuario_actual).ToList();
                    ViewBag.usuario_val = usuario_actual;
                }

                if (tphorario == "persona")
                {
                    if (val != null)
                    {
                        parametrizacion_horario horario = db.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == val);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", val);
                            //ViewBag.usuario_id = db.users.Where(x => x.user_id == val).ToList();
                            ViewBag.usuario_val = val;
                            BuscarFavoritos(menu);
                            return View();
                        }

                        return RedirectToAction("Edit", new { id = horario.horario_id, menu });
                    }
                    else
                    {
                        parametrizacion_horario horario = db.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == usuario_actual);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            BuscarFavoritos(menu);
                            return View();
                        }

                        return RedirectToAction("Edit", new { id = horario.horario_id, menu });
                    }
                }

                if (tphorario == "carro" && val != null)
                {
                    ViewBag.val = "carro";
                    ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", val);
                    parametrizacion_horario horario = db.parametrizacion_horario.FirstOrDefault(x => x.demo_id == val);
                    if (horario == null)
                    {
                        BuscarFavoritos(menu);
                        return View();
                    }

                    return RedirectToAction("Edit", new { id = horario.horario_id, menu });
                }

                BuscarFavoritos(menu);
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        // POST: parametrizacionHorario/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(parametrizacion_horario parametrizacion_horario, int? menu)
        {
            if (ModelState.IsValid)
            {
                parametrizacion_horario.fecha_creacion = DateTime.Now;
                parametrizacion_horario.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.parametrizacion_horario.Add(parametrizacion_horario);
                db.SaveChanges();

                int idhorario = db.parametrizacion_horario.OrderByDescending(x => x.horario_id).FirstOrDefault()
                    .horario_id;
                if (!string.IsNullOrEmpty(Request["lista_novedades"]))
                {
                    int listaNovedades = Convert.ToInt32(Request["lista_novedades"]);

                    for (int i = 1; i <= listaNovedades + 1; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["desde" + i]))
                        {
                            vhorarionovedad novedad = new vhorarionovedad
                            {
                                horarioid = idhorario,
                                fechaini = Convert.ToDateTime(Request["desde" + i]),
                                fechafin = Convert.ToDateTime(Request["hasta" + i]),
                                motivo = Request["motivo" + i]
                            };
                            db.vhorarionovedad.Add(novedad);
                        }
                    }
                }

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización de horario creada correctamente";
            }

            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", Convert.ToInt32(Session["user_usuarioid"]));
            ViewBag.usuario_id = db.users.ToList();
            ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario.demo_id);
            BuscarFavoritos(menu);
            return View();
        }

        // GET: parametrizacionHorario/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            parametrizacion_horario parametrizacion_horario = db.parametrizacion_horario.Find(id);
            if (parametrizacion_horario == null)
            {
                return HttpNotFound();
            }
            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", parametrizacion_horario.usuario_id);
            ViewBag.usuario_id = db.users.ToList();
            ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario.demo_id);
            BuscarFavoritos(menu);
            return View(parametrizacion_horario);
        }

        // POST: parametrizacionHorario/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(parametrizacion_horario parametrizacion_horario, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parametrizacion_horario).State = EntityState.Modified;
                db.SaveChanges();

                if (!string.IsNullOrEmpty(Request["lista_novedades"]))
                {
                    int listaNovedades = Convert.ToInt32(Request["lista_novedades"]);
                    for (int i = 1; i <= listaNovedades + 1; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["desde" + i]))
                        {
                            vhorarionovedad novedad = new vhorarionovedad
                            {
                                horarioid = parametrizacion_horario.horario_id,
                                fechaini = Convert.ToDateTime(Request["desde" + i]),
                                fechafin = Convert.ToDateTime(Request["hasta" + i]),
                                motivo = Request["motivo" + i]
                            };
                            db.vhorarionovedad.Add(novedad);
                        }
                    }
                }

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización de horario actualizado correctamente";
            }

            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", parametrizacion_horario.usuario_id);
            ViewBag.usuario_id = db.users.ToList();
            ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario.demo_id);
            BuscarFavoritos(menu);
            return View(parametrizacion_horario);
        }

        public int EliminarNovedad(int id)
        {
            vhorarionovedad dato = db.vhorarionovedad.Find(id);
            db.Entry(dato).State = EntityState.Deleted;
            int result = db.SaveChanges();
            return result;
        }

        public JsonResult BuscarNovedades(int horarioid)
        {
            var data = from n in db.vhorarionovedad
                       where n.horarioid == horarioid
                       select new
                       {
                           n.horarioid,
                           fechaini = n.fechaini.ToString(),
                           fechafin = n.fechafin.ToString(),
                           n.id,
                           n.motivo
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }
        //////
        ///
       
        ////////////////////////////////////////
        public ActionResult createPlanta(string tphorario, int? val, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                ViewBag.usuario_id = db.users.ToList();
                ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", val);

                if (Session["user_rolid"].ToString() == "4" || Session["user_rolid"].ToString() == "3")
                {
                    //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", usuario_actual);
                    //ViewBag.usuario_id = db.users.Where(x=> x.user_id == usuario_actual).ToList();
                    ViewBag.usuario_val = usuario_actual;
                }

                if (tphorario == "persona")
                {
                    if (val != null)
                    {
                        parametrizacion_horario_planta horario = db.parametrizacion_horario_planta.FirstOrDefault(x => x.usuario_id == val);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", val);
                            //ViewBag.usuario_id = db.users.Where(x => x.user_id == val).ToList();
                            ViewBag.usuario_val = val;
                            BuscarFavoritosPlanta(menu);
                            return View();
                        }

                        return RedirectToAction("EditPlanta", new { id = horario.horario_id, menu });
                    }
                    else
                    {
                        parametrizacion_horario_planta horario = db.parametrizacion_horario_planta.FirstOrDefault(x => x.usuario_id == usuario_actual);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            BuscarFavoritosPlanta(menu);
                            return View();
                        }

                        return RedirectToAction("EditPlanta", new { id = horario.horario_id, menu });
                    }
                }

                //if (tphorario == "carro" && val != null)
                //{
                //    ViewBag.val = "carro";
                //    ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", val);
                //    parametrizacion_horario_planta horario = db.parametrizacion_horario_planta.FirstOrDefault(x => x.demo_id == val);
                //    if (horario == null)
                //    {
                //        BuscarFavoritos(menu);
                //        return View();
                //    }

                //    return RedirectToAction("Edit", new { id = horario.horario_id, menu });
                //}

                BuscarFavoritosPlanta(menu);
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        // POST: parametrizacionHorario/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult createPlanta(parametrizacion_horario_planta parametrizacion_horario_planta, int? menu)
        {
            if (ModelState.IsValid)
            {
                parametrizacion_horario_planta.fecha_creacion = DateTime.Now;
                parametrizacion_horario_planta.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.parametrizacion_horario_planta.Add(parametrizacion_horario_planta);
                db.SaveChanges();

                int idhorario = db.parametrizacion_horario_planta.OrderByDescending(x => x.horario_id).FirstOrDefault()
                    .horario_id;
                if (!string.IsNullOrEmpty(Request["lista_novedades"]))
                {
                    int listaNovedades = Convert.ToInt32(Request["lista_novedades"]);

                    for (int i = 1; i <= listaNovedades + 1; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["desde" + i]))
                        {
                            vhorarionovedad novedad = new vhorarionovedad
                            {
                                horarioid = idhorario,
                                fechaini = Convert.ToDateTime(Request["desde" + i]),
                                fechafin = Convert.ToDateTime(Request["hasta" + i]),
                                motivo = Request["motivo" + i]
                            };
                            db.vhorarionovedad.Add(novedad);
                        }
                    }
                }

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización de horario creada correctamente";
            }

            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", Convert.ToInt32(Session["user_usuarioid"]));
            ViewBag.usuario_id = db.users.ToList();
            //ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario_planta.demo_id);
            BuscarFavoritosPlanta(menu);
            return View();
        }

        // GET: parametrizacionHorario/Edit/5
        public ActionResult EditPlanta(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            parametrizacion_horario_planta parametrizacion_horario_planta = db.parametrizacion_horario_planta.Find(id);
            if (parametrizacion_horario_planta == null)
            {
                return HttpNotFound();
            }
            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", parametrizacion_horario.usuario_id);
            ViewBag.usuario_id = db.users.ToList();
            //ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario.demo_id);
            BuscarFavoritosPlanta(menu);
            return View(parametrizacion_horario_planta);
        }

        // POST: parametrizacionHorario/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPlanta(parametrizacion_horario_planta parametrizacion_horario_planta, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parametrizacion_horario_planta).State = EntityState.Modified;
                db.SaveChanges();

                if (!string.IsNullOrEmpty(Request["lista_novedades"]))
                {
                    int listaNovedades = Convert.ToInt32(Request["lista_novedades"]);
                    for (int i = 1; i <= listaNovedades + 1; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["desde" + i]))
                        {
                            vhorarionovedad novedad = new vhorarionovedad
                            {
                                horarioid = parametrizacion_horario_planta.horario_id,
                                fechaini = Convert.ToDateTime(Request["desde" + i]),
                                fechafin = Convert.ToDateTime(Request["hasta" + i]),
                                motivo = Request["motivo" + i]
                            };
                            db.vhorarionovedad.Add(novedad);
                        }
                    }
                }

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización de horario actualizado correctamente";
            }

            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", parametrizacion_horario.usuario_id);
            ViewBag.usuario_id = db.users.ToList();
            //  ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario.demo_id);
            BuscarFavoritosPlanta(menu);
            return View(parametrizacion_horario_planta);
        }

        public int EliminarNovedadPlanta(int id)
        {
            vhorarionovedad dato = db.vhorarionovedad.Find(id);
            db.Entry(dato).State = EntityState.Deleted;
            int result = db.SaveChanges();
            return result;
        }

        public JsonResult BuscarNovedadesPlanta(int horarioid)
        {
            var data = from n in db.vhorarionovedad
                       where n.horarioid == horarioid
                       select new
                       {
                           n.horarioid,
                           fechaini = n.fechaini.ToString(),
                           fechafin = n.fechafin.ToString(),
                           n.id,
                           n.motivo
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

     

        public void BuscarFavoritosPlanta(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }

        public JsonResult BuscarHorarioAsesorPlanta()
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
            //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
            string sortColumn = Request.Form
                .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                .FirstOrDefault();
            string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            search = search.Replace(" ", "");
            int pagina = Convert.ToInt32(start);
            int pageSize = Convert.ToInt32(length);
            int skip = 0;
            if (pagina == 0)
            {
                skip = 0;
            }
            else
            {
                skip = pagina;
            }

            int asesorLog = Convert.ToInt32(Session["user_usuarioid"]);

            #region Si el que esta consultando es un asesor

            int rol = Convert.ToInt32(Session["user_rolid"]);

            string parametroAsesor = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
            string parametroAsesorU = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;
            //inicializo el predicado
            var predicado = PredicateBuilder.True<vw_Horarios_Asesores_Planta>();
            if (rol == Convert.ToInt32(parametroAsesor) || rol == Convert.ToInt32(parametroAsesorU))
            {

                predicado = predicado.And(d => d.usuario_id == asesorLog);
            }
            //predicado = predicado.And(d => d.turno == true);
            var registrostotales = db.vw_Horarios_Asesores_Planta.Where(predicado).Count();

            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                var buscarTerceros = db.vw_Horarios_Asesores_Planta.Where(predicado).OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var dataHorarioPlanta = buscarTerceros.Select(x => new
                {
                    horario_id = x.horario_id,
                    tipo_horario = x.tipo_horario,
                    nombreasesorcompleto = x.nombreasesorcompleto,
                    lunes_total = x.lunes_total,
                    martes_total = x.martes_total,
                    miercoles_total = x.miercoles_total,
                    jueves_total = x.jueves_total,
                    viernes_total = x.viernes_total,
                    sabado_total = x.sabado_total,
                    domingo_total = x.domingo_total,

                turno = x.turno == true ? "Disponible" : "No disponible",
                checks = x.checks == true ? "Disponible" : "No disponible",
                }).ToList();

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = dataHorarioPlanta },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscarTerceros = db.vw_Horarios_Asesores_Planta.Where(predicado).OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var dataHorarioPlanta = buscarTerceros.Select(x => new
                {
                    horario_id = x.horario_id,
                    tipo_horario = x.tipo_horario,
                    nombreasesorcompleto = x.nombreasesorcompleto,
                    lunes_total = x.lunes_total,
                    martes_total = x.martes_total,
                    miercoles_total = x.miercoles_total,
                    jueves_total = x.jueves_total,
                    viernes_total = x.viernes_total,
                    sabado_total = x.sabado_total,
                    domingo_total = x.domingo_total,
                    turno = x.turno,
                    checks = x.checks,


                }).ToList();

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = dataHorarioPlanta },
                    JsonRequestBehavior.AllowGet);
            }



            #endregion
        }
        //////////////////////////////////
        ///
        public ActionResult createPrueba(string tphorario, int? val, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                ViewBag.usuario_id = db.users.ToList();
                ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", val);

                if (Session["user_rolid"].ToString() == "4" || Session["user_rolid"].ToString() == "3")
                {
                    //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", usuario_actual);
                    //ViewBag.usuario_id = db.users.Where(x=> x.user_id == usuario_actual).ToList();
                    ViewBag.usuario_val = usuario_actual;
                }

                if (tphorario == "persona")
                {
                    if (val != null)
                    {
                        horario_planta_asesor horario = db.horario_planta_asesor.FirstOrDefault(x => x.asesor_id == val);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", val);
                            //ViewBag.usuario_id = db.users.Where(x => x.user_id == val).ToList();
                            ViewBag.usuario_val = val;
                            BuscarFavoritosPlanta(menu);
                            return View();
                        }

                        return RedirectToAction("EditPlanta", new { id = horario.horario_id, menu });
                    }
                    else
                    {
                        parametrizacion_horario_planta horario = db.parametrizacion_horario_planta.FirstOrDefault(x => x.usuario_id == usuario_actual);
                        if (horario == null)
                        {
                            ViewBag.val = "persona";
                            BuscarFavoritosPlanta(menu);
                            return View();
                        }

                        return RedirectToAction("EditPrueba", new { id = horario.horario_id, menu });
                    }
                }

              

                BuscarFavoritosPlanta(menu);
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        // POST: parametrizacionHorario/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult createPrueba(horario_planta_asesor horario_planta, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.horario_planta_asesor.Add(horario_planta);
                db.SaveChanges();

                
                if (!string.IsNullOrEmpty(Request["numerodias"]))
                {
                    int dias = Convert.ToInt32(Request["numerodias"]);

                    for (int i = 1; i <= dias; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["desde" + i]))
                        {
                            var horario = new horario_planta_asesor();
                            //capturo campo fecha
                            var fechax = Request["fecha" + i];
                            var fechax1 = Request["hora_desde" + i];
                            var fechax2 = Request["hora_hasta" + i];
                            var fechax3 = Request["hora_desde1" + i];
                            var fechax4 = Request["hora_hasta1" + i];

                            var fechan = DateTime.Now;
                            var fechan1 = TimeSpan.Zero;
                            var fechan2 = TimeSpan.Zero;
                            var fechan3 = TimeSpan.Zero;
                            var fechan4 = TimeSpan.Zero;

                            var convertir = DateTime.TryParse(fechax, out fechan);
                            var desdex1= TimeSpan.TryParse(fechax1, out fechan1);
                            var hastax1 = TimeSpan.TryParse(fechax2, out fechan2);
                            var desdex2 = TimeSpan.TryParse(fechax3, out fechan3);
                            var hastax2 = TimeSpan.TryParse(fechax4, out fechan4);

                            //hago transformaciones de las horar para hora desde hora hasta hora desde2 y hora hasta 2
                            //cuidando si son valores nulos o no.
                            //busco si existe la combinacion de asesor y fecha

                            var existe = db.horario_planta_asesor.Where(d => d.asesor_id == horario_planta.asesor_id && d.fecha == fechan).FirstOrDefault();
                            if (existe == null)
                            {
   
                               horario.asesor_id = horario_planta.asesor_id;
                                horario.fecha = horario_planta.fecha;
                                horario.hora_desde = fechan1;
                                horario.hora_hasta = fechan2;
                                horario.hora_desde2 = fechan3;
                                horario.hora_hasta2 = fechan4;
                                horario.disponible = horario.disponible;


                                db.horario_planta_asesor.Add(horario);
                            }
                            else
                            {
                                horario.asesor_id = horario_planta.asesor_id;
                                horario.fecha = horario_planta.fecha;
                                horario.hora_desde = horario_planta.hora_desde;
                                horario.hora_hasta = horario_planta.hora_hasta;
                                horario.hora_desde2 = horario_planta.hora_desde2;
                                horario.hora_hasta2 = horario_planta.hora_hasta2;
                                horario.disponible = horario.disponible;
                                db.Entry(horario).State = EntityState.Modified;
                            }
                            
                        }
                    }
                    var guardar = db.SaveChanges();
                }

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización de horario creada correctamente";
            }

            //ViewBag.usuario_id = new SelectList(db.users, "user_id", "user_nombre", Convert.ToInt32(Session["user_usuarioid"]));
            ViewBag.usuario_id = db.users.ToList();
            //ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa", parametrizacion_horario_planta.demo_id);
            BuscarFavoritosPlanta(menu);
            return View();
        }


        public JsonResult buscarPlantaHorario(int? asesor, DateTime? fecha, DateTime? hasta)
        {
            var resultado = 0;
            var error = "";
            int dias = 0;
            List<horarioPlantaModel> lista = new List<horarioPlantaModel>();
            //verifico que ninguna de las tres variables es nula
            if (asesor != null && fecha != null && hasta != null)
            {
                //verifico si el asesor existe y está activo
                var estadoasesor = db.users.Where(x => x.user_id == asesor).FirstOrDefault();
                var buscarasesor = db.parametrizacion_horario_planta.Where(x => x.usuario_id == asesor).FirstOrDefault();
                TimeSpan calcular = hasta.Value.Date - fecha.Value.Date;
                 dias = calcular.Days;
                if (estadoasesor.user_estado)
                {
                    var diaactual = fecha;
                    for (int i = 0; i <= dias; i++)
                    {
                        diaactual = fecha.Value.AddDays(i);
                        lista.Add(new horarioPlantaModel
                        {
                            fecha = diaactual.Value,
                            fecha2=diaactual.Value.ToString("yyyy/MM/dd",new CultureInfo("en-US")),
                            asesorid = asesor.Value,
                        });

                    }

                    //voy a ver si existe el horario en tabla
                    foreach (var item in lista)
                    {
                        //buscamos si existe el asesor, y la fecha en la tabla de base de datos que guarda el horario en planta.
                        var existe = db.horario_planta_asesor.Where(x => x.asesor_id == asesor && x.fecha == fecha).FirstOrDefault();
                        if (existe != null)
                        {                            
                            item.disponible = existe.disponible;
                            item.hora_desde = existe.hora_desde!=null? existe.hora_desde.Value.ToString("HH:mm", new CultureInfo("en-US")) :" ";
                            item.hora_hasta = existe.hora_hasta!=null?existe.hora_hasta.Value.ToString("HH:mm", new CultureInfo("en-US")) :" ";
                            item.hora_desde1 = existe.hora_desde2!=null?existe.hora_desde2.Value.ToString("HH:mm", new CultureInfo("en-US")) :" ";
                            item.hora_hasta1 = existe.hora_hasta2!=null?existe.hora_hasta2.Value.ToString("HH:mm", new CultureInfo("en-US")) :" ";

                        }
                        resultado = 1;
                    }
                }


            }
            else
            {
                error = "valores nulos";
            }

            //veo diferencia de dias entre fechas


            var data = new
            {
                datos = lista,
                result=resultado,
                mensaje=error,
                diass= dias,
            };

            return Json(data, JsonRequestBehavior.AllowGet);

        }



    }
}


