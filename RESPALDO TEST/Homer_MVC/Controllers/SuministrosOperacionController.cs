using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class SuministrosOperacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: SuministrosOperacion
        public ActionResult Index(int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                //
                listasdesplegables();
                BuscarFavoritos(menu);
                return View();
            }

            return RedirectToAction("Login", "login");
        }

        public JsonResult buscar(int? modelo, string operacion, string referencia)
        {
            System.Linq.Expressions.Expression<Func<tsuministromanoot, bool>> predicado = PredicateBuilder.True<tsuministromanoot>();
            if (modelo != null)
            {
                predicado = predicado.And(d => d.modelo_general == modelo);
            }

            if (!string.IsNullOrWhiteSpace(operacion))
            {
                predicado = predicado.And(d => d.operacion == operacion);
            }

            if (!string.IsNullOrWhiteSpace(referencia))
            {
                predicado = predicado.And(d => d.referencia == referencia);
            }

            System.Collections.Generic.List<tsuministromanoot> lista = context.tsuministromanoot.Where(predicado).ToList();
            var data = lista.Select(d => new
            { id = d.id_suministromanoot, d.modelo_general, d.referencia, d.operacion, d.cantidad }).ToList();
            return Json(data);
        }


        public void listasdesplegables()
        {
            //me traigo las operaciones
            var temparios = context.ttempario.Where(d => d.estado)
                .Select(d => new { id = d.codigo, nombre = d.operacion }).ToList();
            ViewBag.tempario = new SelectList(temparios, "id", "nombre");

            //los repuestos
            var repuestos = context.icb_referencia.Where(d => d.modulo == "R" && d.ref_estado)
                .Select(d => new { id = d.ref_codigo, nombre = d.ref_descripcion }).ToList();
            ViewBag.repuesto = new SelectList(repuestos, "id", "nombre");

            //los modelos generales
            var modelos = context.vmodelog.Where(d => d.estado).Select(d => new { d.id, nombre = d.Descripcion })
                .ToList();
            ViewBag.modelo = new SelectList(modelos, "id", "nombre");
            ViewBag.modalModelo = new SelectList(modelos, "id", "nombre");
        }

        public JsonResult operacionesModelo(int? modelo)
        {
            if (modelo != null)
            {
                //busco las operaciones asignadas a ese modelo
                //var operaciones = context.tsuministromanoot.Where(d => d.modelo_general == modelo).Select(d => d.operacion).ToList();
                //ahora busco de todos los temparios los que AUN no tenga asignados
                var operaciones2 = context.ttempario.Where(d => d.estado /*&& !operaciones.Contains(d.codigo)*/)
                    .Select(d => new { id = d.codigo, nombre = d.operacion }).ToList();
                return Json(operaciones2);
            }

            return Json(0);
        }

        public JsonResult ReferenciasModelo(int? modelo, string operacion)
        {
            if (modelo != null && !string.IsNullOrWhiteSpace(operacion))
            {
                //busco las referencias asignadas a ese modelo y operaion
                System.Collections.Generic.List<string> operaciones = context.tsuministromanoot
                    .Where(d => d.modelo_general == modelo && d.operacion == operacion).Select(d => d.referencia)
                    .ToList();
                //ahora busco de todos los temparios los que AUN no tenga asignados
                var operaciones2 = context.icb_referencia
                    .Where(d => d.ref_estado && !operaciones.Contains(d.ref_codigo))
                    .Select(d => new { id = d.ref_codigo, nombre = d.ref_descripcion }).ToList();
                return Json(operaciones2);
            }

            return Json(0);
        }

        public JsonResult nuevo(int? modelo, string operacion, string referencia, int? cantidad)
        {
            if (modelo != null && !string.IsNullOrWhiteSpace(operacion) && !string.IsNullOrWhiteSpace(referencia) &&
                cantidad != null)
            {
                //busco si el modelo general existe
                vmodelog modelog = context.vmodelog.Where(d => d.id == modelo).FirstOrDefault();
                //busco si la operación existe
                ttempario operaciong = context.ttempario.Where(d => d.codigo == operacion).FirstOrDefault();
                //busco si la referencia existe
                icb_referencia referenciag = context.icb_referencia.Where(d => d.ref_codigo == referencia).FirstOrDefault();
                if (modelog != null && operaciong != null && referenciag != null)
                {
                    //busco si ya existe la combinación en la tabla 
                    int relaciong = context.tsuministromanoot.Where(d =>
                        d.modelo_general == modelo && d.operacion == operacion && d.referencia == referencia).Count();
                    if (relaciong == 0)
                    {
                        try
                        {
                            //creo la nueva relación
                            tsuministromanoot rela = new tsuministromanoot
                            {
                                modelo_general = modelo.Value,
                                operacion = operacion,
                                referencia = referencia,
                                cantidad = cantidad.Value
                            };
                            context.tsuministromanoot.Add(rela);
                            int guardar = context.SaveChanges();
                            var data = new { resultado = "exito" };
                            return Json(data);
                        }
                        catch (Exception e)
                        {
                            var data = new { resultado = e.Message };

                            return Json(data);
                        }
                    }

                    {
                        var data = new { resultado = "La relación no existe" };

                        return Json(data);
                    }
                }

                {
                    var data = new { resultado = "No existe en BD la referencia, la operación o el modelo ingresados" };
                    return Json(data);
                }
            }

            {
                var data = new { resultado = "Campos Vacíos" };
                return Json(data);
            }
        }

        public JsonResult actualizar(long? id)
        {
            if (id != null)
            {
                tsuministromanoot rela = context.tsuministromanoot.Where(d => d.id_suministromanoot == id).FirstOrDefault();
                if (rela != null)
                {
                    var temparios = context.ttempario.Where(d => d.estado)
                        .Select(d => new { id = d.codigo, nombre = d.operacion }).ToList();
                    //los repuestos
                    var repuestos = context.icb_referencia.Where(d => d.modulo == "R" && d.ref_estado)
                        .Select(d => new { id = d.ref_codigo, nombre = d.ref_descripcion }).ToList();
                    //los modelos generales
                    var modelos = context.vmodelog.Where(d => d.estado).Select(d => new { d.id, nombre = d.Descripcion })
                        .ToList();
                    var data = new
                    {
                        id,
                        rela.operacion,
                        modelo = rela.modelo_general,
                        rela.referencia,
                        rela.cantidad,
                        temparios,
                        repuestos,
                        modelos
                    };
                    return Json(data);
                }

                return Json(-1);
            }

            return Json(0);
        }

        public JsonResult actualizarGuardar(long? id, int? modelo, string operacion, string referencia, int? cantidad)
        {
            if (modelo != null && !string.IsNullOrWhiteSpace(operacion) && !string.IsNullOrWhiteSpace(referencia) &&
                cantidad != null)
            {
                //busco si el modelo general existe
                vmodelog modelog = context.vmodelog.Where(d => d.id == modelo).FirstOrDefault();
                //busco si la operación existe
                ttempario operaciong = context.ttempario.Where(d => d.codigo == operacion).FirstOrDefault();
                //busco si la referencia existe
                icb_referencia referenciag = context.icb_referencia.Where(d => d.ref_codigo == referencia).FirstOrDefault();
                //verifico si la combinacion de modelo, operacion y referencia ya existe para otro id
                int existeotro = context.tsuministromanoot.Where(d =>
                    d.modelo_general == modelo && d.operacion == operacion && d.referencia == referencia &&
                    d.id_suministromanoot != id).Count();
                if (existeotro == 0)
                {
                    tsuministromanoot sumi = context.tsuministromanoot.Where(d => d.id_suministromanoot == id).FirstOrDefault();
                    sumi.operacion = operacion;
                    sumi.referencia = referencia;
                    sumi.modelo_general = modelo.Value;
                    sumi.cantidad = cantidad.Value;
                    context.Entry(sumi).State = EntityState.Modified;
                    context.SaveChanges();
                    var data = new { resultado = "exito" };
                    return Json(data);
                }
                else
                {
                    var data = new
                    { resultado = "Ya existe esa combinación de modelo, operación y referencia guardada en BD" };

                    return Json(data);
                }
            }

            {
                var data = new { resultado = "Campos vacíos, no se puede guardar" };

                return Json(data);
            }
        }

        public JsonResult eliminar(long? id)
        {
            if (id != null)
            {
                tsuministromanoot rela = context.tsuministromanoot.Where(d => d.id_suministromanoot == id).FirstOrDefault();
                if (rela != null)
                {
                    context.Entry(rela).State = EntityState.Deleted;
                    context.SaveChanges();
                    return Json(1);
                }

                return Json(-1);
            }

            return Json(0);
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