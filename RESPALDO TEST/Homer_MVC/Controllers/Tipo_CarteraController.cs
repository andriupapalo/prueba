using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class Tipo_CarteraController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: Tipo_Cartera
        public ActionResult Create(int? menu)
        {
            ViewBag.perfil_contable = new SelectList(context.perfil_contable_documento.OrderBy(x => x.descripcion),
                "id", "descripcion");

            var centro_costo = context.centro_costo.Select(x => new
            {
                value = x.centcst_id,
                text = x.pre_centcst + " - " + x.centcst_nombre
            }).OrderBy(x => x.text).ToList();
            BuscarFavoritos(menu);
            ViewBag.centro_costo = new SelectList(centro_costo, "value", "text");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Tipos_Cartera nuevo, int? menu)
        {
            int valida = context.Tipos_Cartera.Where(x => x.descripcion == nuevo.descripcion).Count();
            if (valida == 0)
            {
                nuevo.fec_creacion = DateTime.Now;
                nuevo.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Tipos_Cartera.Add(nuevo);
                context.SaveChanges();
                TempData["mensaje"] = "La creación del tipo de cartera se realizó exitosamente";
                return View(nuevo);
            }

            TempData["mensaje_error"] = "Ya existe otro tipo de cartera con estos mismos datos, no fue posible crear";
            BuscarFavoritos(menu);
            return View(nuevo);
        }


        public ActionResult Browser()
        {
            return View();
        }

        public ActionResult Edit(int? menu, int id)
        {
            Tipos_Cartera buscar = context.Tipos_Cartera.Find(id);
            if (buscar != null)
            {
                ViewBag.descripcion = buscar.descripcion;
            }

            return View(buscar);
        }

        [HttpPost]
        public ActionResult Edit(Tipos_Cartera actualizar, int? menu)
        {
            int buscarOtro = context.Tipos_Cartera
                .Where(x => x.descripcion == actualizar.descripcion && x.id != actualizar.id).Count();
            if (buscarOtro > 0)
            {
                TempData["mensaje_error"] =
                    "Ya existe otro tipo de cartera con estos mismos datos, no fue posible actualizar";

                BuscarFavoritos(menu);
                ViewBag.descripcion = actualizar.descripcion;
                return View();
            }

            Tipos_Cartera buscar = context.Tipos_Cartera.Find(actualizar.id);
            buscar.descripcion = actualizar.descripcion;
            buscar.fec_actualizacion = DateTime.Now;
            buscar.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            buscar.estado = actualizar.estado;
            context.Entry(buscar).State = EntityState.Modified;
            context.SaveChanges();
            TempData["mensaje"] = "Se realizó la actualización exitosamente";

            BuscarFavoritos(menu);
            ViewBag.descripcion = actualizar.descripcion;
            return View();
        }

        public JsonResult datos()
        {
            var buscar = context.Tipos_Cartera.Select(x => new
            {
                x.id,
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
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