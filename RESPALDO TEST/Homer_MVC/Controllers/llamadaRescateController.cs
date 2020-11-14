using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class llamadaRescateController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: llamadaRescate
        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Index(icb_registro_llamadas llamadas, int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarProspectos()
        {
            var prospectos = (from t in context.icb_terceros
                              join vh in context.icb_tercero_vhretoma
                                  on t.tercero_id equals vh.tercero_id
                              join s in context.icb_statusprospecto on t.estado_prospecto equals s.status_id into ps
                              from s in ps.DefaultIfEmpty()
                              where t.prospecto
                                    && !context.icb_cotizacion.Any(x => x.id_tercero == t.tercero_id)
                              select new
                              {
                                  t.tercero_id,
                                  t.prinom_tercero,
                                  t.segnom_tercero,
                                  t.apellido_tercero,
                                  t.segapellido_tercero,
                                  t.tercerofec_creacion,
                                  regllam_prox_fecha = t.fecha_proximo_contacto != null ? t.fecha_proximo_contacto.ToString() : "",
                                  status_nombre = s.status_nombre != null ? s.status_nombre : " "
                              }).Distinct().ToList();
            var data = new
            {
                prospectos
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCotizaciones()
        {
            var cotizacion = (from t in context.icb_terceros
                              join vh in context.icb_tercero_vhretoma
                                  on t.tercero_id equals vh.tercero_id
                              join s in context.icb_statusprospecto on t.estado_prospecto equals s.status_id into pr
                              from s in pr.DefaultIfEmpty()
                              join c in context.icb_cotizacion
                                  on t.tercero_id equals c.id_tercero
                              where t.prospecto
                                    && c.cot_negocio == null
                              select new
                              {
                                  t.tercero_id,
                                  t.prinom_tercero,
                                  t.segnom_tercero,
                                  t.apellido_tercero,
                                  t.segapellido_tercero,
                                  //c.cot_vehidescripcion,
                                  regllam_prox_fecha = t.fecha_proximo_contacto != null ? t.fecha_proximo_contacto.ToString() : "",
                                  status_nombre = s.status_nombre != null ? s.status_nombre : " ",
                                  c.cot_idserial
                              }).ToList();
            var data = new
            {
                cotizacion
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatos(int id)
        {
            var datos = (from t in context.icb_terceros
                         join vh in context.icb_tercero_vhretoma
                             on t.tercero_id equals vh.tercero_id
                         join c in context.icb_cotizacion
                             on t.tercero_id equals c.id_tercero into pr
                         from c in pr.DefaultIfEmpty()
                         where t.tercero_id == id
                         select new
                         {
                             t.tercero_id,
                             t.prinom_tercero,
                             t.segnom_tercero,
                             t.apellido_tercero,
                             t.segapellido_tercero,
                             t.tercerofec_creacion,
                             t.email_tercero,
                             t.telf_tercero,
                             t.celular_tercero,
                             cot_idserial = c.cot_idserial == 0 ? c.cot_idserial.ToString() : ""
                         }).ToList();

            var lisllamadas = (from ll in context.icb_registro_llamadas
                               join u in context.users
                                   on ll.regllam_usuela equals u.user_id
                               where ll.tercero_id == id
                               select new
                               {
                                   ll.regllam_verbalizacion,
                                   u.user_nombre,
                                   u.user_apellido,
                                   regllam_prox_fecha = ll.regllam_prox_fecha != null ? ll.regllam_prox_fecha.ToString() : "",
                                   regllam_fecela = ll.regllam_fecela != null ? ll.regllam_fecela.ToString() : ""
                               }).ToList();

            var data = new
            {
                datos,
                lisllamadas
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Crear_registro(int id, int? menu)
        {
            registroLlamadasModel modelo = new registroLlamadasModel { tercero_id = id };
            ViewBag.idtecero = id;
            ViewBag.tpllamada_rescate_id =
                new SelectList(context.icb_tpllamadarescate, "tpllamada_id", "tpllamada_nombre");
            ViewBag.statusprospecto_id = new SelectList(context.icb_statusprospecto, "status_id", "status_nombre");
            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Crear_registro(registroLlamadasModel llamadas, int? menu)
        {
            if (ModelState.IsValid)
            {
                string idcotizacion = Request["idcotizacion"];
                if (idcotizacion != null)
                {
                    llamadas.cotizacion_id = Convert.ToInt32(idcotizacion);
                }

                ViewBag.idtecero = llamadas.tercero_id;
                llamadas.regllam_fecela = DateTime.Now;
                llamadas.regllam_usuela = Convert.ToInt32(Session["user_usuarioid"]);
                context.icb_registro_llamadas.Add(new icb_registro_llamadas
                {
                    regllam_verbalizacion = llamadas.regllam_verbalizacion,
                    regllam_prox_fecha = llamadas.regllam_prox_fecha,
                    regllam_fecela = llamadas.regllam_fecela,
                    tercero_id = llamadas.tercero_id,
                    cotizacion_id = llamadas.cotizacion_id,
                    statusprospecto_id = llamadas.statusprospecto_id,
                    tpllamada_rescate_id = llamadas.tpllamada_rescate_id,
                    regllam_usuela = llamadas.regllam_usuela
                });
                bool resul = context.SaveChanges() > 0;

                if (resul)
                {
                    icb_terceros tercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == llamadas.tercero_id);
                    tercero.fecha_proximo_contacto = llamadas.regllam_prox_fecha;
                    tercero.estado_prospecto = llamadas.statusprospecto_id;
                    context.Entry(tercero).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["mensaje"] = "Registro creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error al crear el registro de llamadas, por favor intente nuevamente";
                }
            }

            ViewBag.tpllamada_rescate_id =
                new SelectList(context.icb_tpllamadarescate, "tpllamada_id", "tpllamada_nombre");
            ViewBag.statusprospecto_id = new SelectList(context.icb_statusprospecto, "status_id", "status_nombre");
            BuscarFavoritos(menu);
            return View(llamadas);
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