using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class checkListUsadosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: checkListUsados
        public ActionResult Create(int? menu)
        {
            vingresovehiculo modelo = new vingresovehiculo { estado = true, nuevo = true };
            BuscarFavoritos(menu);
            ViewBag.aplica = new SelectList(context.tipo_Checklist.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion");
            return View(modelo);
        }


        [HttpPost]
        public ActionResult Create(vingresovehiculo modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                vingresovehiculo existe = context.vingresovehiculo.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (existe == null)
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.tipoCheckid = Convert.ToInt32(Request["aplica"]);
                    context.vingresovehiculo.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        if (modelo.tiporespuesta == "select")
                        {
                            string opciones = Request["opcionRespuesta"];
                            if (!string.IsNullOrEmpty(opciones))
                            {
                                vingresovehiculo buscarUltimoIngreso = context.vingresovehiculo.OrderByDescending(x => x.id)
                                    .FirstOrDefault();

                                string[] opcion = opciones.Split(',');
                                foreach (string substring in opcion)
                                {
                                    if (!string.IsNullOrEmpty(substring.Trim()))
                                    {
                                        context.vingresovehiculoopcion.Add(new vingresovehiculoopcion
                                        {
                                            id_ingreso = buscarUltimoIngreso.id,
                                            descripcion = substring,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            fec_creacion = DateTime.Now
                                        });
                                    }
                                }
                            }

                            int guardarOpciones = context.SaveChanges();
                            if (guardarOpciones > 0)
                            {
                                TempData["mensaje"] = "El registro del nuevo parametro de check list fue exitoso!";
                            }
                        }
                        else
                        {
                            TempData["mensaje"] = "El registro del nuevo parametro de check list fue exitoso!";
                        }
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Ya existe un parametro con el mismo nombre, por favor valide!";
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error de conexion con la base de datos!";
            }

            ViewBag.aplica = new SelectList(context.tipo_Checklist.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion");
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vingresovehiculo vIngreso = context.vingresovehiculo.Find(id);
            if (vIngreso == null)
            {
                return HttpNotFound();
            }

            ViewBag.tipoRespuesta = vIngreso.tiporespuesta.Trim();
            ViewBag.aplica = new SelectList(context.tipo_Checklist.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", vIngreso.tipoCheckid);
            ConsultarDatosCreacion(vIngreso);
            BuscarFavoritos(menu);
            return View(vIngreso);
        }


        [HttpPost]
        public ActionResult update(vingresovehiculo modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                vingresovehiculo buscarVIngreso = context.vingresovehiculo.Find(modelo.id);
                if (buscarVIngreso != null)
                {
                    buscarVIngreso.descripcion = modelo.descripcion;
                    buscarVIngreso.tiporespuesta = modelo.tiporespuesta;
                    buscarVIngreso.nuevo = modelo.nuevo;
                    buscarVIngreso.usado = modelo.usado;
                    buscarVIngreso.estado = modelo.estado;
                    buscarVIngreso.razon_inactivo = modelo.razon_inactivo;
                    buscarVIngreso.fec_actualizacion = DateTime.Now;
                    buscarVIngreso.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarVIngreso.tipoCheckid = Convert.ToInt32(Request["aplica"]);
                    context.Entry(buscarVIngreso).State = EntityState.Modified;
                }

                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    const string query = "DELETE FROM [dbo].[vingresovehiculoopcion] WHERE [id_ingreso]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.id);

                    if (modelo.tiporespuesta == "select")
                    {
                        string opciones = Request["opcionRespuesta"];
                        if (!string.IsNullOrEmpty(opciones))
                        {
                            string[] opcion = opciones.Split(',');
                            foreach (string substring in opcion)
                            {
                                if (!string.IsNullOrEmpty(substring.Trim()))
                                {
                                    context.vingresovehiculoopcion.Add(new vingresovehiculoopcion
                                    {
                                        id_ingreso = modelo.id,
                                        descripcion = substring,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        fec_creacion = DateTime.Now
                                    });
                                }
                            }
                        }

                        int guardarOpciones = context.SaveChanges();
                        if (guardarOpciones > 0)
                        {
                            TempData["mensaje"] = "El registro del nuevo parametro de check list fue actualizado!";
                        }
                    }

                    TempData["mensaje"] = "El registro del nuevo parametro de check list fue actualizado!";
                }
                else
                {
                    TempData["mensaje"] = "Error de conexion con la base de datos!";
                }
            }

            ViewBag.tipoRespuesta = modelo.tiporespuesta.Trim();
            ViewBag.aplica = new SelectList(context.tipo_Checklist.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", Convert.ToInt32(Request["aplica"]));
            ConsultarDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultarDatosCreacion(vingresovehiculo ingreso)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(ingreso.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(ingreso.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarParametrosCheck()
        {
            var buscarParametros = (from parametros in context.vingresovehiculo
                                    join tipo in context.tipo_Checklist
                                        on parametros.tipoCheckid equals tipo.id into xx
                                    from tipo in xx.DefaultIfEmpty()
                                    select new
                                    {
                                        parametros.descripcion,
                                        parametros.id,
                                        tipoVehiculos = tipo.descripcion != null ? tipo.descripcion : "",
                                        tipo = parametros.tiporespuesta,
                                        estado = parametros.estado ? "Activo" : "Inactivo"
                                    }).ToList();

            return Json(buscarParametros, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarParametrosExistentes(int? id_parametro)
        {
            var buscarParametros = (from opciones in context.vingresovehiculoopcion
                                    where opciones.id_ingreso == id_parametro
                                    select new
                                    {
                                        opciones.id,
                                        opciones.descripcion
                                    }).ToList();

            if (buscarParametros != null)
            {
                return Json(new { existenParametros = true, buscarParametros }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { existenParametros = false }, JsonRequestBehavior.AllowGet);
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