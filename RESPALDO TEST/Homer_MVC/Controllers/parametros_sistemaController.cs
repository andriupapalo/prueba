using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class parametros_sistemaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        //necesito un objeto que gestione la tabla


        private static Expression<Func<icb_sysparameter, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(icb_sysparameter), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<icb_sysparameter, string>> lambda = Expression.Lambda<Func<icb_sysparameter, string>>(menuProperty, menu);

            return lambda;
        }

        // GET: parametros_sistema
        public ActionResult Crear(int? menu)
        {
            icb_sysparameter parametro = new icb_sysparameter { syspar_estado = true };
            //var buscarParametros = context.icb_sysparameter.ToList();
            //ViewBag.datosTabla = buscarParametros;
            BuscarFavoritos(menu);
            return View(parametro);
        }


        public JsonResult BuscarTiemposConsultas()
        {
            var buscar = (from tiempos in context.diaspaginacion
                          select new
                          {
                              tiempos.id,
                              tiempos.Descripcion,
                              tiempos.dias
                          }).ToList();
            return Json(buscar, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AsignarParametro(int? menu)
        {
            int id_tablaDias = Convert.ToInt32(Request["parametroDias"]);
            icb_sysparameter parametroDias = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P54");
            if (parametroDias != null)
            {
                parametroDias.syspar_value = id_tablaDias.ToString();
                context.Entry(parametroDias).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "Se asignó el parametro correctamente!";
            }
            else
            {
                TempData["mensaje_error"] = "No existe un parametro establecido para guardar este valor!";
            }

            return RedirectToAction("TiempoConsultas", new { menu });
        }


        public ActionResult TiempoConsultas(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.parametroDias = new SelectList(context.diaspaginacion, "id", "Descripcion");
            icb_sysparameter idParametroDias = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P54");
            ViewBag.idDiasSeleccionado = idParametroDias != null ? idParametroDias.syspar_value : "";
            return View(new diaspaginacion { estado = true });
        }


        [HttpPost]
        public ActionResult TiempoConsultas(diaspaginacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                diaspaginacion buscarNombre =
                    context.diaspaginacion.FirstOrDefault(x =>
                        x.Descripcion == modelo.Descripcion || x.dias == modelo.dias);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del parametro o el numero de dias ya esta registrado!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.diaspaginacion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del parametro de días se guardo exitosamente!";
                        BuscarFavoritos(menu);
                        ViewBag.parametroDias = new SelectList(context.diaspaginacion, "id", "Descripcion");
                        return RedirectToAction("TiempoConsultas", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            BuscarFavoritos(menu);
            ViewBag.parametroDias = new SelectList(context.diaspaginacion, "id", "Descripcion");
            icb_sysparameter idParametroDias = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P54");
            ViewBag.idDiasSeleccionado = idParametroDias != null ? idParametroDias.syspar_value : "";
            return View(modelo);
        }


        public ActionResult ActualizaTiempoConsulta(int id, int? menu)
        {
            diaspaginacion dias = context.diaspaginacion.Find(id);
            if (dias == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacionParamDias(dias);
            BuscarFavoritos(menu);
            return View(dias);
        }


        [HttpPost]
        public ActionResult ActualizaTiempoConsulta(diaspaginacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                diaspaginacion buscarNombre =
                    context.diaspaginacion.FirstOrDefault(x =>
                        x.Descripcion == modelo.Descripcion || x.dias == modelo.dias);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.estado = modelo.estado;
                        buscarNombre.Descripcion = modelo.Descripcion;
                        buscarNombre.dias = modelo.dias;
                        buscarNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del parametro de días fue exitosa!";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                        }
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del parametro de días fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
            }

            ConsultaDatosCreacionParamDias(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        [HttpPost]
        public ActionResult Crear(icb_sysparameter modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_sysparameter buscaCodigoExiste = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == modelo.syspar_cod);
                if (buscaCodigoExiste != null)
                {
                    TempData["mensaje_error"] = "El codigo del parametro ya esta registrado";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                modelo.sysparfec_creacion = DateTime.Now;
                modelo.sysparuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.icb_sysparameter.Add(modelo);
                bool guardarParametro = context.SaveChanges() > 0;
                if (guardarParametro)
                {
                    TempData["mensaje"] = "El parametro se creo exitosamente";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }
            }

            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Actualizar(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_sysparameter buscarParametoPorId = context.icb_sysparameter.FirstOrDefault(x => x.syspar_id == id);
            if (buscarParametoPorId == null)
            {
                return HttpNotFound();
            }

            // Significa que si se encontro el id del parametro
            users creador = context.users.Find(buscarParametoPorId.sysparuserid_creacion);

            ViewBag.user_nombre_cre = creador.user_nombre + " " + creador.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificador = context.users.Find(buscarParametoPorId.sysparuserid_actualizacion);
            if (modificador != null)
            {
                ViewBag.user_nombre_act = modificador.user_nombre + " " + modificador.user_apellido;
            }

            BuscarFavoritos(menu);
            return View(buscarParametoPorId);
        }


        [HttpPost]
        public ActionResult Actualizar(icb_sysparameter modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_sysparameter buscaCodigoYValor = context.icb_sysparameter.FirstOrDefault(x =>
                    x.syspar_cod == modelo.syspar_cod && x.syspar_id == modelo.syspar_id);
                if (buscaCodigoYValor != null)
                {
                    string descAnteriorNueva = buscaCodigoYValor.syspar_description + "|" + modelo.syspar_description;
                    string valorAnteriorNuevo = buscaCodigoYValor.syspar_value + "|" + modelo.syspar_value;

                    buscaCodigoYValor.syspar_cod = modelo.syspar_cod;
                    buscaCodigoYValor.syspar_value = modelo.syspar_value;
                    buscaCodigoYValor.syspar_description = modelo.syspar_description;
                    buscaCodigoYValor.syspar_estado = modelo.syspar_estado;
                    buscaCodigoYValor.sysparrazoninactivo = modelo.sysparrazoninactivo;
                    buscaCodigoYValor.sysparuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscaCodigoYValor.sysparfec_actualizacion = DateTime.Now;
                    context.Entry(buscaCodigoYValor).State = EntityState.Modified;
                    bool guardarParametro = context.SaveChanges() > 0;

                    context.icb_sysparameter_log.Add(new icb_sysparameter_log
                    {
                        paramlog_fecha = DateTime.Now,
                        paramlog_usuario = Convert.ToInt32(Session["user_usuarioid"]),
                        paramlog_descripcion = descAnteriorNueva,
                        paramlog_valor = valorAnteriorNuevo
                    });


                    bool guardarLog = context.SaveChanges() > 0;
                    modelo.sysparuserid_actualizacion = buscaCodigoYValor.sysparuserid_actualizacion;
                    modelo.sysparfec_actualizacion = buscaCodigoYValor.sysparfec_actualizacion;
                    if (guardarParametro && guardarLog)
                    {
                        ConsultaDatosCreacion(modelo);
                        TempData["mensaje"] = "El parametro ha sido actualizado correctamente";
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    ConsultaDatosCreacion(modelo);
                    TempData["mensaje_error"] = "Error de conexion, intente mas tarde";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                ConsultaDatosCreacion(modelo);
                TempData["mensaje_error"] = "El codigo que ingreso ya se encuentra registrado";
                BuscarFavoritos(menu);
                return View(modelo);
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(icb_sysparameter modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.sysparuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(modelo.sysparuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public void ConsultaDatosCreacionParamDias(diaspaginacion modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(modelo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetParametrosJson()
        {
            System.Collections.Generic.List<icb_sysparameter> buscarParametros = context.icb_sysparameter.ToList();
            return Json(buscarParametros, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetParametrosJsonPaginados(string filtroGeneral)
        {
            //columnas que vienen del datatable
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

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            //filtrado por el campo a buscar. 
            Expression<Func<icb_sysparameter, bool>> predicado = PredicateBuilder.True<icb_sysparameter>();
            Expression<Func<icb_sysparameter, bool>> predicado2 = PredicateBuilder.False<icb_sysparameter>();
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado2 = predicado2.Or(d => 1 == 1 && d.syspar_cod.Contains(filtroGeneral));
                predicado2 = predicado2.Or(d => 1 == 1 && d.syspar_value.Contains(filtroGeneral));
                predicado2 = predicado2.Or(d => 1 == 1 && d.syspar_description.Contains(filtroGeneral));
                predicado = predicado.And(predicado2);
            }

            //cuento el TOTAL de registros sin el filtrado
            int registrostotales = context.icb_sysparameter.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                var query = context.icb_sysparameter.Where(predicado).OrderBy(GetColumnName(sortColumn).Compile())
                    .Skip(skip).Take(pageSize).Select(d => new
                    {
                        d.syspar_cod,
                        d.syspar_value,
                        d.syspar_description,
                        d.syspar_id
                    }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                var query = context.icb_sysparameter.Where(predicado)
                    .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).Select(d => new
                    {
                        d.syspar_cod,
                        d.syspar_value,
                        d.syspar_description,
                        d.syspar_id
                    }).ToList();
                int contador = query.Count();

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
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
    }
}