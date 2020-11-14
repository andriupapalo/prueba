using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tareasAsignadasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tareasAsignadas/Create
        public ActionResult BrowserTareas(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult buscarDatos()
        {
            var info = (from ta in context.tareasasignadas
                        join c in context.icb_cotizacion
                            on ta.idcotizacion equals c.cot_idserial
                        join tc in context.icb_terceros
                            on c.id_tercero equals tc.tercero_id
                        join b in context.bodega_concesionario
                            on c.bodegaid equals b.id
                        join u in context.users
                            on ta.userid_creacion equals u.user_id
                        select new
                        {
                            ta.id,
                            ta.idcotizacion,
                            c.asesor,
                            tc.tercero_id,
                            clienteT = "(" + tc.doc_tercero + ") " + tc.prinom_tercero + " " + tc.segnom_tercero + " " +
                                       tc.apellido_tercero + " " + tc.segapellido_tercero,
                            clienteE = "(" + tc.doc_tercero + ") " + tc.razon_social,
                            tc.telf_tercero,
                            tc.celular_tercero,
                            tc.email_tercero,
                            b.bodccs_nombre,
                            solicitado = u.user_nombre + " " + u.user_apellido,
                            ta.fec_creacion,
                            ta.notas,
                            context.terceros_direcciones.OrderByDescending(x => x.id)
                                .FirstOrDefault(x => x.idtercero == tc.tercero_id).direccion
                        }).ToList();

            var data = info.Select(x => new
            {
                x.id,
                x.idcotizacion,
                x.asesor,
                x.tercero_id,
                cliente = x.clienteT != null ? x.clienteT : x.clienteE,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                bodega = x.bodccs_nombre != null ? x.bodccs_nombre : "",
                solicitado = x.solicitado != null ? x.solicitado : "",
                fecha = x.fec_creacion != null ? x.fec_creacion.ToString("yyyy/MM/dd") : "",
                notas = x.notas != null ? x.notas : "",
                direccion = x.direccion != null ? x.direccion : ""
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarTipificaciones()
        {
            var data = context.tipificaciontercero.Where(x => x.estado).OrderBy(x => x.descripcion).ToList().Select(x =>
                new
                {
                    x.id,
                    x.descripcion
                });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarTipificacion(int idTarea, int tipificacion, string nota)
        {
            tareasasignadas existe = context.tareasasignadas.FirstOrDefault(x => x.id == idTarea);
            if (existe != null)
            {
                existe.idtipificacion = tipificacion;
                existe.observaciones = nota;
                context.Entry(existe).State = EntityState.Modified;
                int result = context.SaveChanges();

                if (result > 0)
                {
                    return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
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