using Homer_MVC.IcebergModel;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class accesoriosModeloController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Create(int? menu)
        {
            // El tipo 2 hace referencia a las referencias que son de tipo accesorio

            var referencias = context.icb_referencia.Where(x => x.tipo_id == 2).Select(x => new
            {
                value = x.ref_codigo,
                text = x.ref_codigo + " " + x.ref_descripcion
            }).ToList();

            ViewBag.accesorios = new SelectList(referencias, "value", "text");
            ViewBag.modeloid = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(vaccesoriomodelo modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                vaccesoriomodelo buscarModelo = context.vaccesoriomodelo.FirstOrDefault(x => x.modeloid == modelo.modeloid);
                if (buscarModelo != null)
                {
                    TempData["mensaje_error"] =
                        "El modelo seleccionado seleccionado ya tiene asignado accesorios, puede modificarlo en la opcion busquedas";
                }
                else
                {
                    string accesorios = Request["accesorios"];
                    if (!string.IsNullOrEmpty(accesorios))
                    {
                        string[] accesoriosId = accesorios.Split(',');

                        foreach (string substring in accesoriosId)
                        {
                            var buscaReferencia = (from referencia in context.icb_referencia
                                                   where referencia.ref_codigo == substring
                                                   select new
                                                   {
                                                       referencia.ref_descripcion,
                                                       referencia.precio_venta
                                                   }).FirstOrDefault();
                            context.vaccesoriomodelo.Add(new vaccesoriomodelo
                            {
                                modeloid = modelo.modeloid,
                                referencia = substring,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                descripcion = buscaReferencia.ref_descripcion,
                                precio = buscaReferencia.precio_venta,
                                estado = true
                            });
                        }

                        int result = context.SaveChanges();
                        if (result > 0)
                        {
                            TempData["mensaje"] = "Los accesorios del modelo se han creado exitosamente";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Debe registrar minimo un accesorio para el modelo seleccionado";
                    }
                }
            }

            // El tipo 2 hace referencia a las referencias que son de tipo accesorio
            var referencias = context.icb_referencia.Where(x => x.tipo_id == 2).Select(x => new
            {
                value = x.ref_codigo,
                text = x.ref_codigo + " " + x.ref_descripcion
            }).ToList();

            ViewBag.accesorios = new SelectList(referencias, "value", "text");
            ViewBag.modeloid = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult update(string id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            System.Collections.Generic.List<vaccesoriomodelo> accesorios = context.vaccesoriomodelo.Where(x => x.modeloid == id).ToList();
            if (accesorios.Count == 0)
            {
                return HttpNotFound();
            }
            // El tipo 2 hace referencia a las referencias que son de tipo accesorio
            ViewBag.accesorios = new SelectList(context.icb_referencia.Where(x => x.tipo_id == 2), "ref_codigo",
                "ref_descripcion");
            string modelo = accesorios.FirstOrDefault().modeloid;
            ViewBag.modeloid = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre", modelo);

            vaccesoriomodelo buscaActualizacion = context.vaccesoriomodelo.OrderByDescending(x => x.id_accesorio_modelo)
                .FirstOrDefault(x => x.modeloid == id);
            ConsultaDatosCreacion(buscaActualizacion);
            BuscarFavoritos(menu);
            return View(buscaActualizacion);
        }


        [HttpPost]
        public ActionResult update(vaccesoriomodelo modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                string modeloId = modelo.modeloid;
                string accesorios = Request["accesorios"];
                if (!string.IsNullOrEmpty(accesorios))
                {
                    string[] accesoriosId = accesorios.Split(',');
                    const string query = "DELETE FROM [dbo].[vaccesoriomodelo] WHERE [modeloid]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.modeloid);

                    foreach (string substring in accesoriosId)
                    {
                        var buscaReferencia = (from referencia in context.icb_referencia
                                               where referencia.ref_codigo == substring
                                               select new
                                               {
                                                   referencia.ref_descripcion,
                                                   referencia.precio_venta
                                               }).FirstOrDefault();
                        context.vaccesoriomodelo.Add(new vaccesoriomodelo
                        {
                            modeloid = modelo.modeloid,
                            referencia = substring,
                            fec_creacion = modelo.fec_creacion,
                            userid_creacion = modelo.userid_creacion,
                            user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                            fec_actualizacion = DateTime.Now,
                            descripcion = buscaReferencia.ref_descripcion,
                            precio = buscaReferencia.precio_venta,
                            estado = true
                        });
                    }

                    int result = context.SaveChanges();
                    if (result > 0)
                    {
                        TempData["mensaje"] = "Los accesorios del modelo se han actualizado exitosamente";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Debe registrar minimo un accesorio para el modelo seleccionado";
                }
            }
            else
            {
                System.Collections.Generic.List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            // El tipo 2 hace referencia a las referencias que son de tipo accesorio
            ViewBag.accesorios = new SelectList(context.icb_referencia.Where(x => x.tipo_id == 2), "ref_codigo",
                "ref_descripcion");
            ViewBag.modeloid = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre", modelo.modeloid);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(vaccesoriomodelo modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(modelo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult GetAccesoriosActual(string idModelo)
        {
            System.Collections.Generic.List<vaccesoriomodelo> buscarAccesorios2 = context.vaccesoriomodelo.Where(x => x.modeloid == idModelo).ToList();
            var buscarAccesorios = buscarAccesorios2.Select(x => new
            {
                id = x.referencia,
                nombre = nombreaccesorio(x.referencia)
            }).ToList();
            return Json(buscarAccesorios, JsonRequestBehavior.AllowGet);
        }

        public string nombreaccesorio(string referencia)
        {
            string resultado = "";
            icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == referencia).FirstOrDefault();
            if (refer != null)
            {
                resultado = refer.ref_descripcion;
            }

            return resultado;
        }

        public JsonResult BuscarModelosPaginadas()
        {
            var buscarModelosConAccesorio = (from accesorio in context.vaccesoriomodelo
                                             join modelo in context.modelo_vehiculo
                                                 on accesorio.modeloid equals modelo.modvh_codigo
                                             select new
                                             {
                                                 modelo.modvh_codigo,
                                                 modelo.modvh_nombre
                                             }).Distinct().ToList();
            return Json(buscarModelosConAccesorio, JsonRequestBehavior.AllowGet);
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