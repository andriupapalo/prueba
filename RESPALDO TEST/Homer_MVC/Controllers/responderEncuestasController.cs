using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class responderEncuestasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: responderEncuestas
        public ActionResult Index(int? menu, string plan_mayor, string parametro)
        {
            var list = (from t in db.icb_terceros
                        select new
                        {
                            t.tercero_id,
                            nombre = t.doc_tercero + " - " + t.razon_social + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                     t.apellido_tercero + " " + t.segapellido_tercero,
                            order = t.prinom_tercero + t.razon_social
                        }).OrderBy(x => x.order).ToList();

            if (plan_mayor == "" || plan_mayor == null)
            {
                SelectList lista = new SelectList(list, "tercero_id", "nombre");
                ViewBag.tercero = lista;
                ViewBag.plan_mayor = "";
            }
            else
            {
                int? buscarTercero = db.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == plan_mayor).propietario;
                SelectList lista = new SelectList(list, "tercero_id", "nombre", buscarTercero);
                ViewBag.tercero = lista;
                ViewBag.plan_mayor = plan_mayor;
            }

            if (parametro == null || parametro == "")
            {
                ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion");
            }
            else
            {
                string param = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == parametro).syspar_value;
                ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion", param);
            }


            ViewBag.parametro = parametro;
            ViewBag.encuestasRealizadas = db.crm_encuesta_respuestas.ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult LlamadaRescate(int? menu)
        {
            ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion");
            ViewBag.tercero = new SelectList(db.icb_terceros, "tercero_id", "doc");
            //ViewBag.datos = db.icb_terceros.ToList();
            ViewBag.encuestasRealizadas = db.crm_encuesta_respuestas.ToList();
            DateTime hoy = DateTime.Now.AddHours(-2);
            ViewBag.datos = db.icb_terceros.Where(x =>
                x.tercerofec_creacion > hoy && x.tercerofec_creacion.Day == DateTime.Now.Day &&
                x.tercerofec_creacion.Year == DateTime.Now.Year).ToList();
            BuscarFavoritos(menu);
            return View();
        }


        // GET: responderEncuestas/Create
        public ActionResult Create(int tercero, int encuesta, int? menu)
        {
            crm_encuesta_respuestas respuestas = new crm_encuesta_respuestas
            {
                id_encuesta = encuesta,
                tercero = tercero
            };
            if (ModelState.IsValid)
            {
                //db.crm_encuesta_respuestas.Add(crm_encuesta_respuestas);
                db.SaveChanges();
            }
            //return RedirectToAction("Index");

            ViewBag.tercero = new SelectList(db.icb_terceros, "tercero_id", "doc_tercero");
            ViewBag.encuestador = new SelectList(db.users, "user_id", "user_nombre");
            ViewBag.id = new SelectList(db.crm_encuestas_detalle_respuestas, "id_respuesta", "respuesta");
            ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion");
            System.Collections.Generic.List<crm_preguntas> preguntas = db.crm_preguntas.Where(x => x.id_encu == encuesta).ToList();
            ViewBag.preguntas = preguntas;
            BuscarFavoritos(menu);
            return View(respuestas);
        }

        // POST: responderEncuestas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(crm_encuesta_respuestas respuestas, int? menu)
        {
            int value = Convert.ToInt32(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P69").syspar_value);
            crm_encuesta_respuestas buscar = db.crm_encuesta_respuestas.FirstOrDefault(x =>
                x.id_encuesta == value && x.tercero == respuestas.tercero && x.plan_mayor == respuestas.plan_mayor);
            if (buscar == null)
            {
                respuestas.encuestador = Convert.ToInt32(Session["user_usuarioid"]);
                string plan_mayor = Request["plan_mayor"];
                if (plan_mayor != "")
                {
                    respuestas.plan_mayor = plan_mayor;
                }

                respuestas.fecha = DateTime.Now;
                db.crm_encuesta_respuestas.Add(respuestas);
                db.SaveChanges();

                int idencab = respuestas.id;
                System.Collections.Generic.List<crm_preguntas> listaRespuestas = db.crm_preguntas.Where(x => x.id_encu == respuestas.id_encuesta).ToList();
                foreach (crm_preguntas item in listaRespuestas)
                {
                    crm_encuestas_detalle_respuestas rta = new crm_encuestas_detalle_respuestas
                    {
                        id_encab_respuesta = idencab,
                        id_pregunta = item.id,
                        respuesta = Request["respuesta_" + item.id],
                        comentario = Request["comentario_" + item.id]
                    };
                    db.crm_encuestas_detalle_respuestas.Add(rta);
                    db.SaveChanges();
                }

                TempData["mensaje"] = "Encuesta guardada correctamente";

                var encabezado = (from a in db.crm_encuesta_respuestas
                                  join encuesta in db.crm_encuestas
                                      on a.id_encuesta equals encuesta.id
                                  join tercero in db.icb_terceros
                                      on a.tercero equals tercero.tercero_id
                                  join users in db.users
                                      on a.encuestador equals users.user_id
                                  where a.id == idencab
                                  select new
                                  {
                                      a.id,
                                      nombreEncuesta = encuesta.Descripcion,
                                      a.fecha,
                                      nombreAsesor = users.user_nombre + " " + users.user_apellido,
                                      nombreCliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                      tercero.apellido_tercero + " " + tercero.segapellido_tercero
                                  }).FirstOrDefault();

                encuestaPDFmodel datos = new encuestaPDFmodel
                {
                    nombreEncuesta = encabezado.nombreEncuesta,
                    fecha = encabezado.fecha.ToString("yyyy/MM/dd HH:mm:ss"),
                    nombreAsesor = encabezado.nombreAsesor,
                    nombreCliente = encabezado.nombreCliente,
                    preguntas = (from a in db.crm_encuestas_detalle_respuestas
                                 join b in db.crm_preguntas
                                     on a.id_pregunta equals b.id
                                 where a.id_encab_respuesta == idencab
                                 select new preguntasPDF
                                 {
                                     pregunta = b.pregunta,
                                     respuesta = a.respuesta
                                 }).ToList()
                };
                ViewAsPdf something = new ViewAsPdf("encuesta", datos);
                return something;
            }
            else
            {
                TempData["mensaje_error"] = "Ya hay una encuesta registrada al cliente con el mismo plan mayor";
                var encabezado = (from a in db.crm_encuesta_respuestas
                                  join encuesta in db.crm_encuestas
                                      on a.id_encuesta equals encuesta.id
                                  join tercero in db.icb_terceros
                                      on a.tercero equals tercero.tercero_id
                                  join users in db.users
                                      on a.encuestador equals users.user_id
                                  where a.id == buscar.id
                                  select new
                                  {
                                      a.id,
                                      nombreEncuesta = encuesta.Descripcion,
                                      a.fecha,
                                      nombreAsesor = users.user_nombre + " " + users.user_apellido,
                                      nombreCliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                      tercero.apellido_tercero + " " + tercero.segapellido_tercero
                                  }).FirstOrDefault();

                encuestaPDFmodel datos = new encuestaPDFmodel
                {
                    nombreEncuesta = encabezado.nombreEncuesta,
                    fecha = encabezado.fecha.ToString("yyyy/MM/dd HH:mm:ss"),
                    nombreAsesor = encabezado.nombreAsesor,
                    nombreCliente = encabezado.nombreCliente,
                    preguntas = (from a in db.crm_encuestas_detalle_respuestas
                                 join b in db.crm_preguntas
                                     on a.id_pregunta equals b.id
                                 where a.id_encab_respuesta == buscar.id
                                 select new preguntasPDF
                                 {
                                     pregunta = b.pregunta,
                                     respuesta = a.respuesta
                                 }).ToList()
                };
                ViewAsPdf something = new ViewAsPdf("encuesta", datos);
                return something;
            }

            //return RedirectToAction("Index", new { menu });
        }

        public ActionResult Detalles(int id, int? menu)
        {
            System.Collections.Generic.List<crm_encuestas_detalle_respuestas> detalles = db.crm_encuestas_detalle_respuestas.Where(x => x.id_encab_respuesta == id).ToList();
            crm_encuesta_respuestas datos = db.crm_encuesta_respuestas.Find(id);
            ViewBag.cliente = datos.icb_terceros.prinom_tercero + " " + datos.icb_terceros.segnom_tercero + "" +
                              datos.icb_terceros.apellido_tercero + " " + datos.icb_terceros.segapellido_tercero;
            ViewBag.encuesta = datos.crm_encuestas.Descripcion;
            BuscarFavoritos(menu);
            return View(detalles);
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
    }
}