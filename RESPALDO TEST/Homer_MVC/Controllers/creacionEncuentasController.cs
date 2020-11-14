using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class creacionEncuentasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void listas()
        {
            ViewBag.modulo = new SelectList(context.crm_encuesta_modulo, "id", "Descripcion");
            ViewBag.encuesta = new SelectList(context.crm_encuestas, "id", "Descripcion");
        }

        // GET: creacionEncuentas
        public ActionResult Index(int? menu)
        {
            listas();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(int? menu)
        {
            string preguntas = Request["listas_preguntas"];

            if (!string.IsNullOrEmpty(preguntas))
            {
                int result = 0;
                int numPreguntas = Convert.ToInt32(preguntas);
                for (int i = 1; i <= numPreguntas; i++)
                {
                    if (!string.IsNullOrEmpty(Request["pregunta_" + i]))
                    {
                        crm_preguntas p = new crm_preguntas
                        {
                            id_encu = Convert.ToInt32(Request["encuesta"]),
                            pregunta = Request["pregunta_" + i],
                            tiporespuesta = Request["tipo_respuesta"],
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true
                        };
                        context.crm_preguntas.Add(p);
                        result = context.SaveChanges();

                        int idpregunta = context.crm_preguntas.OrderByDescending(x => x.id).FirstOrDefault().id;

                        string opciones = Request["opcionrta_" + i];
                        string[] lista_opciones = opciones.Split(',');

                        string adicionales = Request["adicional_opcionrta_" + i];
                        string[] lista_adicionales = adicionales.Split(',');

                        for (int j = 0; j < lista_opciones.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(lista_opciones[j].Trim()))
                            {
                                crm_opcionesrespuesta opcionesrta = new crm_opcionesrespuesta
                                {
                                    id_pregunta = idpregunta,
                                    descripcion = lista_opciones[j].Trim(),
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    estado = true,
                                    adicional =
                                    !string.IsNullOrEmpty(lista_adicionales[j].Trim()) ? true : false,
                                    descripcion1 = !string.IsNullOrEmpty(lista_adicionales[j].Trim())
                                    ? lista_adicionales[j].Trim()
                                    : null
                                };
                                context.crm_opcionesrespuesta.Add(opcionesrta);
                                result = context.SaveChanges();
                            }
                        }
                    }
                }

                if (result > 0)
                {
                    TempData["mensaje"] = "Encuesta guardada correctamente";
                    return RedirectToAction("Edit", new { id = Convert.ToInt32(Request["encuesta"]), menu });
                }
            }

            //TempData["mensaje"] = "Errores en la creacion de la encuenta, por favor valide";
            listas();
            return RedirectToAction("Index", new { menu });
        }

        public ActionResult Browser(int? menu)
        {
            ViewBag.datos = context.crm_encuestas.OrderBy(x => x.id).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Edit(int id, int? menu)
        {
            ViewBag.preguntas = context.crm_preguntas.Where(x => x.id_encu == id).OrderBy(x => x.id).ToList();
            //ViewBag.opcionesrta = context.crm_opcionesrespuesta.Where(x=> x.id_pregunta = )
            crm_encuestas encuesta = context.crm_encuestas.Find(id);
            ViewBag.encuesta = new SelectList(context.crm_encuestas, "id", "Descripcion", encuesta.id);
            BuscarFavoritos(menu);
            return View(encuesta);
        }

        public int EliminarPregunta(int id)
        {
            System.Collections.Generic.List<crm_opcionesrespuesta> opciones = context.crm_opcionesrespuesta.Where(x => x.id_pregunta == id).ToList();
            foreach (crm_opcionesrespuesta item in opciones)
            {
                context.Entry(item).State = EntityState.Deleted;
            }

            crm_preguntas dato = context.crm_preguntas.Find(id);
            context.Entry(dato).State = EntityState.Deleted;
            int result = context.SaveChanges();

            return result;
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