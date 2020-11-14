using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class perfilTributarioController : Controller
    {

        private readonly Iceberg_Context context = new Iceberg_Context();


        public void CamposListasDesplegables()
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion");
            ViewBag.tipo_regimenid = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
            ViewBag.conc_regimenid = new SelectList(context.con_ret_del_reg.OrderBy(x => x.descripcion_con), "id_concepto", "descripcion_con");
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.autorretencion = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
        }

        // GET: perfilTributario
        public ActionResult Create(int? menu)
        {
            CamposListasDesplegables();
            ModeloPerfilTributario modelo = new ModeloPerfilTributario() { estado = true, razon_inactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Create(ModeloPerfilTributario modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodega"];
            if (ModelState.IsValid)
            {

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    List<int> listaBodegas = new List<int>();
                    foreach (string item in bodegasId)
                    {
                        listaBodegas.Add(Convert.ToInt32(item));
                    }
                    var buscarPerfil = (from perfilTibutario in context.perfiltributario
                                        join bodega in context.bodega_concesionario
                                        on perfilTibutario.bodega equals bodega.id
                                        where listaBodegas.Contains(perfilTibutario.bodega) && perfilTibutario.sw == modelo.sw && perfilTibutario.tipo_regimenid == modelo.tipo_regimenid
                                        select new
                                        {
                                            bodega.bodccs_nombre
                                        }).FirstOrDefault();
                    if (buscarPerfil != null)
                    {
                        TempData["mensaje_error"] = "El perfil ya se encuentra registrado para la bodega " + buscarPerfil.bodccs_nombre;
                    }
                    else
                    {
                        foreach (int bodegaId in listaBodegas)
                        {
                            perfiltributario nuevoperfil = new perfiltributario
                            {
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_creacion = DateTime.Now,
                                bodega = bodegaId,
                                tipo_regimenid = modelo.tipo_regimenid,
                                sw = modelo.sw,
                                retfuente = modelo.retfuente,
                                retiva = modelo.retiva,
                                retica = modelo.retica,

                                pretfuente = modelo.pretfuente,
                                pretiva = modelo.pretiva,
                                pretica = modelo.pretica,

                                baseretfuente = modelo.baseretfuente,
                                baseretiva = modelo.baseretiva,
                                baseretica = modelo.baseretica,

                                autorretencion = modelo.autorretencion,
                                estado = modelo.estado,
                                razon_inactivo = modelo.razon_inactivo
                            };

                            context.perfiltributario.Add(nuevoperfil);
                        }
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El perfil ha sido creado exitosamente";
                            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                            return RedirectToAction("Create");
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de base de datos, por favor revise su conexion";
                        }
                    }
                }
            }

            CamposListasDesplegables();
            BuscarFavoritos(menu);
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            return View(modelo);
        }

        public ActionResult update(int? id, int? menu)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            perfiltributario perfil = context.perfiltributario.FirstOrDefault(x => x.perfilTributario_id == id);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            var buscarBodegas = from perfilTributario in context.perfiltributario
                                where perfilTributario.sw == perfil.sw && perfilTributario.tipo_regimenid == perfil.tipo_regimenid
                                select new { perfilTributario.bodega, perfilTributario.perfilTributario_id };
            string bodegasString = "";
            string perfil_idString = "";
            bool primera = true;
            foreach (var item in buscarBodegas)
            {
                if (primera)
                {
                    bodegasString += item.bodega;
                    perfil_idString += item.perfilTributario_id;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.bodega;
                    perfil_idString += "," + item.perfilTributario_id;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;
            ViewBag.perfilesSeleccionados = perfil_idString;
            CamposListasDesplegables();
            ConsultaDatosCreacion(perfil);
            BuscarFavoritos(menu);
            return View(perfil);
        }

        [HttpPost]
        public ActionResult update(perfiltributario modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodega"];
            string perfilesSeleccionados = Request["perfilesSeleccionados"];
            if (ModelState.IsValid)
            {

                List<int> listaPerfiles = new List<int>();
                if (!string.IsNullOrEmpty(perfilesSeleccionados))
                {
                    string[] perfilesId = perfilesSeleccionados.Split(',');


                    foreach (string item in perfilesId)
                    {
                        listaPerfiles.Add(Convert.ToInt32(item));
                    }
                }

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    string[] bodegasId = bodegasSeleccionadas.Split(',');

                    List<int> listaBodegas = new List<int>();
                    foreach (string item in bodegasId)
                    {
                        listaBodegas.Add(Convert.ToInt32(item));
                    }

                    int swAnterior = Convert.ToInt32(Request["swAnterior"]);
                    int regimenAnterior = Convert.ToInt32(Request["regimenAnterior"]);
                    var buscarPerfilesAEliminar = (from perfilTributario in context.perfiltributario
                                                   where !listaBodegas.Contains(perfilTributario.bodega) && perfilTributario.sw == swAnterior && perfilTributario.tipo_regimenid == regimenAnterior
                                                   select new { perfilTributario.bodega, perfilTributario.perfilTributario_id }).Distinct().ToList();

                    //var buscarPerfil = (from perfilTibutario in context.perfiltributario
                    //                    join bodega in context.bodega_concesionario
                    //                    on perfilTibutario.bodega equals bodega.id
                    //                    where listaBodegas.Contains(perfilTibutario.bodega) && perfilTibutario.sw == modelo.sw && perfilTibutario.tipo_regimenid == modelo.tipo_regimenid
                    //                    select perfilTibutario).Distinct().ToList();
                    foreach (int bodega in listaBodegas)
                    {
                        perfiltributario buscarRepetido = context.perfiltributario.FirstOrDefault(x => x.bodega == bodega && x.sw == modelo.sw && x.tipo_regimenid == modelo.tipo_regimenid);
                        if (buscarRepetido != null)
                        {
                            if (listaPerfiles.Contains(buscarRepetido.perfilTributario_id))
                            {
                                buscarRepetido.bodega = bodega;
                                buscarRepetido.sw = modelo.sw;
                                buscarRepetido.tipo_regimenid = modelo.tipo_regimenid;
                                buscarRepetido.retfuente = modelo.retfuente;
                                buscarRepetido.retica = modelo.retica;
                                buscarRepetido.retiva = modelo.retiva;
                                //if (!string.IsNullOrWhiteSpace(modelo.pretfuente))
                                //{
                                //    var conver1 = Decimal.TryParse(modelo.pretfuente, out pretfuentenum);
                                //}
                                buscarRepetido.pretfuente = modelo.pretfuente;
                                buscarRepetido.pretica = modelo.pretica;
                                buscarRepetido.pretiva = modelo.pretiva;
                                buscarRepetido.baseretfuente = modelo.baseretfuente;
                                buscarRepetido.baseretica = modelo.baseretica;
                                buscarRepetido.baseretiva = modelo.baseretiva;

                                buscarRepetido.autorretencion = modelo.autorretencion;
                                buscarRepetido.fec_actualizacion = DateTime.Now;
                                buscarRepetido.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                context.Entry(buscarRepetido).State = EntityState.Modified;
                            }
                            else
                            {
                                bodega_concesionario buscaBodega = context.bodega_concesionario.FirstOrDefault(x => x.id == buscarRepetido.bodega);
                                TempData["mensaje_error"] = "El registro del perfil para la bodega " + buscaBodega.bodccs_nombre + " ya se encuentra creado";
                                CamposListasDesplegables();
                                ConsultaDatosCreacion(modelo);
                                BuscarFavoritos(menu);
                                return RedirectToAction("update", new { id = modelo.perfilTributario_id, menu });
                            }
                        }
                        else
                        {
                            context.perfiltributario.Add(new perfiltributario()
                            {
                                bodega = bodega,
                                sw = modelo.sw,
                                tipo_regimenid = modelo.tipo_regimenid,
                                retfuente = modelo.retfuente,
                                retica = modelo.retica,
                                retiva = modelo.retiva,

                                pretfuente = modelo.pretfuente,
                                pretica = modelo.pretica,
                                pretiva = modelo.pretiva,
                                baseretfuente = modelo.baseretfuente,
                                baseretica = modelo.baseretica,
                                baseretiva = modelo.baseretiva,

                                autorretencion = modelo.autorretencion,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_actualizacion = DateTime.Now,
                                user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        }
                    }
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        foreach (var eliminar in buscarPerfilesAEliminar)
                        {
                            const string query = "DELETE FROM [dbo].[perfiltributario] WHERE [perfilTributario_id]={0}";
                            int rows = context.Database.ExecuteSqlCommand(query, eliminar.perfilTributario_id);
                        }

                        TempData["mensaje"] = "La actualización del perfil tributario fue exitoso!";
                        CamposListasDesplegables();
                        ConsultaDatosCreacion(modelo);
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", menu);
                    }
                    else
                    {
                        TempData["mensaje_error"] = "No se ha actualizado ningun registro";
                        CamposListasDesplegables();
                        ConsultaDatosCreacion(modelo);
                        BuscarFavoritos(menu);
                        return RedirectToAction("update", new { id = modelo.perfilTributario_id, menu });
                    }
                }


                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                //var buscar = context.perfiltributario.FirstOrDefault(x=>x.bodega == modelo.bodega && x.sw == modelo.sw && x.tipo_regimenid == modelo.tipo_regimenid);

                //if (buscar!=null)
                //{
                //if (buscar.perfilTributario_id == modelo.perfilTributario_id)
                //{
                //    buscar.bodega = modelo.bodega;
                //    buscar.sw = modelo.sw;
                //    buscar.tipo_regimenid = modelo.tipo_regimenid;
                //    buscar.retfuente = modelo.retfuente;
                //    buscar.retica = modelo.retica;
                //    buscar.retiva = modelo.retiva;
                //    buscar.fec_actualizacion = DateTime.Now;
                //    buscar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                //    context.Entry(buscar).State = EntityState.Modified;
                //    context.SaveChanges();
                //    TempData["mensaje"] = "La actualización del perfil tributario fue exitoso!";
                //    CamposListasDesplegables();
                //    ConsultaDatosCreacion(modelo);
                //    BuscarFavoritos(menu);
                //    return View(buscar);
                //}
                //else {
                //    TempData["mensaje_error"] = "El registro del perfil ya se encuentra creado";
                //}
                //}
                //else
                //{  
                //    modelo.fec_actualizacion = DateTime.Now;
                //    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                //    context.Entry(modelo).State = EntityState.Modified;
                //    context.SaveChanges();
                //    TempData["mensaje"] = "La actualización del perfil tributario fue exitoso!";
                //}
            }
            CamposListasDesplegables();
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }



        public void ConsultaDatosCreacion(perfiltributario modelo)
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






        public JsonResult BuscarPerfilesPaginados()
        {
            var data = from perfil in context.perfiltributario
                       join contributario in context.contributario
                       on perfil.retfuente equals contributario.codigo
                       join contributario2 in context.contributario
                       on perfil.retica equals contributario2.codigo
                       join contributario3 in context.contributario
                       on perfil.retiva equals contributario3.codigo
                       join regimen in context.tpregimen_tercero
                       on perfil.tipo_regimenid equals regimen.tpregimen_id
                       join sw in context.tp_doc_sw
                       on perfil.sw equals sw.tpdoc_id
                       join bodega in context.bodega_concesionario
                       on perfil.bodega equals bodega.id
                       select new
                       {
                           perfil.perfilTributario_id,
                           perfil.bodega,
                           perfil.sw,
                           sw.Descripcion,
                           perfil.tipo_regimenid,
                           regimen.tpregimen_nombre,
                           bodega.bodccs_nombre,
                           retfuente = contributario.descripcion,
                           retica = contributario2.descripcion,
                           retiva = contributario3.descripcion,
                           estado = perfil.estado == true ? "Activo" : "Inactivo",
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }



        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
                                                on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado == true
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
                ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }
            ViewBag.id_menu = menu != null ? menu : 0;
        }



    }
}