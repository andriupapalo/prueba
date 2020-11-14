using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
    {
    public class bahiaController : Controller
        {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: bahia
        public ActionResult Create(int? menu)
            {
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre");
            ViewBag.tipo_bahia = new SelectList(context.ttipobahia, "id", "descripcion");
            ViewBag.tipo_tecnico = new SelectList(context.ttipotecnico, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View(new tbahias { estado = true });
            }


        // POST: bahia
        [HttpPost]
        public ActionResult Create(tbahias modelo, int? menu)
            {

            tbahias buscarSiExiste = context.tbahias.FirstOrDefault(x =>
                x.codigo_bahia == modelo.codigo_bahia && x.bodega == modelo.bodega);
            if (buscarSiExiste == null)
                {
                modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                modelo.fec_creacion = DateTime.Now;
                context.tbahias.Add(modelo);
                var estacheck = Request["bahiavirtual"];
                modelo.bahiavirtual = estacheck != null ? true : false;

                int guardar = context.SaveChanges();
                if (guardar > 0)
                    {
                    tbahias ultimaBahia = context.tbahias.OrderByDescending(x => x.id).FirstOrDefault();
                    context.tcambiobahiatecnico.Add(new tcambiobahiatecnico
                        {
                        idtecnico = modelo.idtecnico ?? 0,
                        bahia = ultimaBahia != null ? ultimaBahia.id : 0,
                        fecha = DateTime.Now,
                        usuario = Convert.ToInt32(Session["user_usuarioid"])
                        });
                    context.SaveChanges();
                    TempData["mensaje"] = "La creación de la bahía fue exitosa";
                    ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre",
                        modelo.bodega);
                    ViewBag.tipo_bahia = new SelectList(context.ttipobahia, "id", "descripcion", modelo.tipo_bahia);
                    ViewBag.tipo_tecnico =
                        new SelectList(context.ttipotecnico, "id", "Descripcion", modelo.tipo_tecnico);
                    BuscarFavoritos(menu);
                    return RedirectToAction("Create", new { menu });
                    }

                TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            else
                {
                TempData["mensaje_error"] =
                    "El codigo de la bahia con la bodega seleccionada ya se encuentra creada, por favor verifique...";
                }


            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", modelo.bodega);
            ViewBag.tipo_bahia = new SelectList(context.ttipobahia, "id", "descripcion", modelo.tipo_bahia);
            ViewBag.tipo_tecnico = new SelectList(context.ttipotecnico, "id", "Descripcion", modelo.tipo_tecnico);
            BuscarFavoritos(menu);
            return View(modelo);
            }


        public ActionResult Edit(int? id, int? menu)
            {
            if (id == null)
                {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

            tbahias bahia = context.tbahias.Find(id);
            if (bahia == null)
                {
                return HttpNotFound();
                }

            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", bahia.bodega);
            ViewBag.tipo_bahia = new SelectList(context.ttipobahia, "id", "descripcion", bahia.tipo_bahia);
            ViewBag.tipo_tecnico = new SelectList(context.ttipotecnico, "id", "Descripcion", bahia.tipo_tecnico);
            ConsultaDatosCreacion(bahia);
            ViewBag.tecnicoSeleccionado = bahia.idtecnico;





            BuscarFavoritos(menu);
            return View(bahia);
            }


        [HttpPost]
        public ActionResult Edit(tbahias modelo, int? menu)
            {
            if (ModelState.IsValid)
                {
                tbahias buscarSiExiste = context.tbahias.FirstOrDefault(x =>
                    x.codigo_bahia == modelo.codigo_bahia && x.bodega == modelo.bodega);
                if (buscarSiExiste == null)
                    {
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_actualizacion = DateTime.Now;
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                        {
                        context.tcambiobahiatecnico.Add(new tcambiobahiatecnico
                            {
                            idtecnico = modelo.idtecnico ?? 0,
                            bahia = modelo.id,
                            fecha = DateTime.Now,
                            usuario = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        context.SaveChanges();
                        TempData["mensaje"] = "La actualización de la bahía fue exitosa";
                        }
                    else
                        {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
                    }
                else
                    {
                    tbahias buscarTecnicoBahia = context.tbahias.FirstOrDefault(x => x.idtecnico == modelo.idtecnico && x.codigo_bahia != modelo.codigo_bahia);

                    if (buscarTecnicoBahia == null || buscarTecnicoBahia.estado == false)
                        {
                        if (buscarSiExiste.id == modelo.id)
                            {
                            modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            modelo.fec_actualizacion = DateTime.Now;
                            buscarSiExiste.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            buscarSiExiste.fec_actualizacion = DateTime.Now;
                            buscarSiExiste.tipo_bahia = modelo.tipo_bahia;
                            buscarSiExiste.bodega = modelo.bodega;
                            buscarSiExiste.tipo_tecnico = modelo.tipo_tecnico;
                            buscarSiExiste.estado = modelo.estado;
                            buscarSiExiste.razon_inactivo = modelo.razon_inactivo;
                            buscarSiExiste.idtecnico = modelo.idtecnico;
                            context.Entry(buscarSiExiste).State = EntityState.Modified;
                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                                {
                                tbahias ultimaBahia = context.tbahias.OrderByDescending(x => x.id).FirstOrDefault();
                                context.tcambiobahiatecnico.Add(new tcambiobahiatecnico
                                    {
                                    idtecnico = modelo.idtecnico ?? 0,
                                    bahia = buscarSiExiste.id,
                                    fecha = DateTime.Now,
                                    usuario = Convert.ToInt32(Session["user_usuarioid"])
                                    });
                                context.SaveChanges();
                                TempData["mensaje"] = "La actualización de la bahía fue exitosa";
                                }
                            else
                                {
                                TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                                }
                            }
                        else
                            {
                            TempData["mensaje_error"] =
                                "El codigo de la bahia con la bodega seleccionada ya se encuentra creada, por favor verifique...";
                            }
                        }
                    else
                        {
                        TempData["mensaje_error"] = "El tecnico seleccionado ya se encuentra registrado en una bahia";


                        }


                    }



                }

            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", modelo.bodega);
            ViewBag.tipo_bahia = new SelectList(context.ttipobahia, "id", "descripcion", modelo.tipo_bahia);
            ViewBag.tipo_tecnico = new SelectList(context.ttipotecnico, "id", "Descripcion", modelo.tipo_tecnico);
            ViewBag.tecnicoSeleccionado = modelo.idtecnico;
            ConsultaDatosCreacion(modelo);

            BuscarFavoritos(menu);
            return View(modelo);
            }


        public void ConsultaDatosCreacion(tbahias bahia)
            {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(bahia.userid_creacion);
            if (creator != null)
                {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
                }

            users modificator = context.users.Find(bahia.user_idactualizacion);
            if (modificator != null)
                {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }


        public JsonResult BuscarBahias()
            {
            var buscarBahias = (from bahia in context.tbahias
                                join tipoBahia in context.ttipobahia
                                    on bahia.tipo_bahia equals tipoBahia.id
                                join bodega in context.bodega_concesionario
                                    on bahia.bodega equals bodega.id
                                join tipoTecnico in context.ttipotecnico
                                    on bahia.tipo_tecnico equals tipoTecnico.id
                                select new
                                    {
                                    bahia.id,
                                    bahia.codigo_bahia,
                                    tipoBahia.descripcion,
                                    bodega.bodccs_nombre,
                                    tipoTecnico.Especializacion,
                                    estado = bahia.estado ? "Activo" : "Inactivo"
                                    }).ToList();

            return Json(buscarBahias, JsonRequestBehavior.AllowGet);
            }


        public JsonResult BuscarTecnicosPorTipo(int? id_tipo_tecnico, int? id_bodega)
            {
            var buscarRolTecnico = (from parametros in context.icb_sysparameter
                                    where parametros.syspar_cod == "P48"
                                    select new
                                        {
                                        parametros.syspar_value
                                        }).FirstOrDefault();
            int idTecnico = buscarRolTecnico != null ? Convert.ToInt32(buscarRolTecnico.syspar_value) : 0;
            var buscarTecnicos = (from tecnicos in context.ttecnicos
                                  join usuarios in context.users
                                      on tecnicos.idusuario equals usuarios.user_id
                                  join bodegaUsuario in context.bodega_usuario
                                      on usuarios.user_id equals bodegaUsuario.id_usuario
                                  join bodega in context.bodega_concesionario
                                      on bodegaUsuario.id_bodega equals bodega.id
                                  where tecnicos.tipo_tecnico == id_tipo_tecnico && bodega.id == id_bodega && usuarios.rol_id == idTecnico
                                  select new
                                      {
                                      tecnicos.id,
                                      nombres = usuarios.user_nombre + " " + usuarios.user_apellido,
                                      usuarios.user_id
                                      }).ToList();

            return Json(buscarTecnicos, JsonRequestBehavior.AllowGet);
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