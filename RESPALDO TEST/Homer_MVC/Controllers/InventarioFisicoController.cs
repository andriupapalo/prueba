using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class InventarioFisicoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public object HttpRemoteDownload { get; private set; }

        //psi

        #region Void/Class

        public void listas(int? id)
        {
            var buscarBodega = (from a in context.ubicacion_repuesto
                                join b in context.bodega_concesionario
                                    on a.bodega equals b.id
                                join c in context.ubicacion_repuestobod
                                    on a.ubicacion equals c.id
                                where c.fisico
                                select new
                                {
                                    b.id,
                                    b.bodccs_nombre
                                }).Distinct().ToList();

            var buscarArea = (from a in context.ubicacion_repuesto
                              join b in context.area_bodega
                                  on a.idarea equals b.areabod_id
                              join c in context.ubicacion_repuestobod
                                  on a.ubicacion equals c.id
                              where c.fisico
                              select new
                              {
                                  b.areabod_id,
                                  b.areabod_nombre
                              }).Distinct().ToList();

            var buscarUbicacion = (from a in context.ubicacion_repuesto
                                   join b in context.ubicacion_bodega
                                       on a.ubicacion equals b.id
                                   join c in context.ubicacion_repuestobod
                                       on a.ubicacion equals c.id
                                   where c.fisico
                                   select new
                                   {
                                       b.id,
                                       b.descripcion
                                   }).Distinct().ToList();

            var buscarReferencia = (from a in context.ubicacion_repuesto
                                    join b in context.icb_referencia
                                        on a.codigo equals b.ref_codigo
                                    join c in context.ubicacion_repuestobod
                                        on a.ubicacion equals c.id
                                    where c.fisico
                                    select new
                                    {
                                        b.ref_codigo,
                                        b.ref_descripcion
                                    }).ToList();

            aencab_auditoriainv modelo = context.aencab_auditoriainv.Find(id);
            if (modelo != null)
            {
                ViewBag.encabezado = modelo.descripcion;
                ViewBag.tipoConteo = Convert.ToString(modelo.tipo_conteo);
                ViewBag.bodegas =
                    new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == modelo.bodega),
                        "id", "bodccs_nombre", modelo.bodega);
                ViewBag.estado_encabezado = Convert.ToInt16(modelo.estado);

                ViewBag.id_encabezado = modelo.id;
                ViewBag.encabezado = modelo.descripcion;
                ViewBag.tipoConteo = Convert.ToString(modelo.tipo_conteo);
                ViewBag.areas = modelo.areas;
                ViewBag.estanterias = modelo.estanterias;
                ViewBag.ubicaciones = modelo.ubicaciones;
                ViewBag.fechaInicia = modelo.fecha_inicio_aInv.ToString("yyyy/MM/dd");
                ViewBag.fechaTermina = modelo.fecha_fin_aInv.ToString("yyyy/MM/dd");
                string[] areas = modelo.areas.Split(',');

                List<areas> nuevaLista = new List<areas>();


                foreach (string item in areas)
                {
                    int id_encab = Convert.ToInt32(item);

                    var buscar_areas = context.area_bodega.Where(x => x.areabod_id == id_encab).Select(x => new
                    {
                        value = x.areabod_id,
                        text = x.areabod_nombre
                    }).ToList();

                    nuevaLista.Add(new areas
                    {
                        value = buscar_areas.Select(x => x.value).FirstOrDefault(),
                        text = buscar_areas.Select(x => x.text).FirstOrDefault()
                    });
                }

                ViewBag.areas_select = new SelectList(nuevaLista, "value", "text");

                var parejas2 = (from a in context.apareja_auditoriainv
                                join b in context.users
                                    on a.id_usuario_1 equals b.user_id
                                join c in context.users
                                    on a.id_usuario_2 equals c.user_id into xx
                                from c in xx.DefaultIfEmpty()
                                where a.id_encabezado == id
                                select new
                                {
                                    a.id,
                                    a.cod_pareja,
                                    persona_1 = b.user_nombre + " " + b.user_apellido,
                                    persona_2 = a.id_usuario_2 != null ? " - " + c.user_nombre + " " + c.user_apellido : ""
                                }).ToList();

                var parejas = parejas2.Select(x => new
                {
                    value = x.id,
                    text = x.cod_pareja + " - " + x.persona_1 + x.persona_2
                });

                ViewBag.parejas_select = new SelectList(parejas, "value", "text");
            }

            ViewBag.bodega_id = new SelectList(buscarBodega, "id", "bodccs_nombre");
            ViewBag.area_id = new SelectList(buscarArea, "areabod_id", "areabod_nombre");
            ViewBag.ubicacion_id = new SelectList(buscarUbicacion, "id", "descripcion");
            ViewBag.referencia_id = new SelectList(buscarReferencia, "ref_codigo", "ref_descripcion");
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

        public class areas
        {
            public int value { get; set; }
            public string text { get; set; }
        }

        public class parejas_ubicaciones
        {
            public int? numero { get; set; }
            public int id_pareja { get; set; }
            public string persona_1 { get; set; }
            public int id_persona_1 { get; set; }
            public string persona_2 { get; set; }
            public int id_persona_2 { get; set; }
            public string ubicaciones { get; set; }
            public string ubicaciones2 { get; set; }
        }

        public class ubicaciones
        {
            public int value { get; set; }
            public string text { get; set; }
        }

        public class conteosExtra
        {
            public int id_conteo { get; set; }
            public string ubicacion { get; set; }
            public int id_ubicacion { get; set; }
            public string ref_codigo { get; set; }
            public string referencia { get; set; }
            public int id_pareja { get; set; }
            public int conteo3 { get; set; }
        }

        #endregion

        #region ActionResult

        #region sin uso 

        // GET: InventarioFisico
        public ActionResult Inventario(int? menu)
        {
            BuscarFavoritos(menu);
            //listas();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inventario(InventarioModel inventario, int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // GET: InventarioFisico
        public ActionResult desbloquear(int? menu)
        {
            BuscarFavoritos(menu);
            //listas();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult desbloquear(InventarioModel inventario, int? menu)
        {
            string[] extraerBodega;
            string[] extraerArea;
            string[] extraerUbicacion;
            string bodegas = Request["bodegaInput"];
            string areas = Request["areaInput"];
            string ubicaciones = Request["ubicacionInput"];

            if (ModelState.IsValid)
            {
                if (bodegas != null || bodegas != "")
                {
                    extraerBodega = bodegas.Split(',');
                    if (areas != null || areas != "")
                    {
                        extraerArea = areas.Split(',');
                        if (ubicaciones != null || ubicaciones != "")
                        {
                            extraerUbicacion = ubicaciones.Split(',');
                            if (extraerBodega.Length == 1 && extraerArea.Length == 1 && extraerUbicacion.Length == 1)
                            {
                                int idBodega = Convert.ToInt32(extraerBodega[0]);
                                int idArea = Convert.ToInt32(extraerArea[0]);
                                int idUbicacion = Convert.ToInt32(extraerUbicacion[0]);

                                List<ubicacion_repuesto> buscar = context.ubicacion_repuesto.Where(x =>
                                    x.bodega == idBodega && x.idarea == idArea && x.ubicacion == idUbicacion).ToList();
                                if (buscar != null && buscar.Count != 0)
                                {
                                    foreach (ubicacion_repuesto item in buscar)
                                    {
                                        // item.fisico = false;
                                        item.fec_actualizacion = DateTime.Now;
                                        item.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        context.Entry(item).State = EntityState.Modified;
                                    }
                                }
                            }
                            else if (extraerBodega.Length == 1 && extraerArea.Length == 1 &&
                                     extraerUbicacion.Length > 1)
                            {
                                foreach (string ubi in extraerUbicacion)
                                {
                                    int idubicacion = Convert.ToInt32(ubi);
                                    int idBodega = Convert.ToInt32(extraerBodega[0]);
                                    int idArea = Convert.ToInt32(extraerArea[0]);
                                    List<ubicacion_repuesto> buscar = context.ubicacion_repuesto.Where(x =>
                                            x.bodega == idBodega && x.idarea == idArea && x.ubicacion == idubicacion)
                                        .ToList();
                                    if (buscar != null && buscar.Count != 0)
                                    {
                                        foreach (ubicacion_repuesto item in buscar)
                                        {
                                            //item.fisico = false;
                                            context.Entry(item).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }
                            else if (extraerBodega.Length == 1 && extraerArea.Length > 1)
                            {
                                foreach (string area in extraerArea)
                                {
                                    int idBodega = Convert.ToInt32(extraerBodega[0]);
                                    int idArea = Convert.ToInt32(area);
                                    List<ubicacion_repuesto> buscar = context.ubicacion_repuesto
                                        .Where(x => x.bodega == idBodega && x.idarea == idArea).ToList();
                                    if (buscar != null && buscar.Count != 0)
                                    {
                                        foreach (ubicacion_repuesto item in buscar)
                                        {
                                            //item.fisico = false;
                                            context.Entry(item).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }
                            else if (extraerBodega.Length > 1)
                            {
                                foreach (string bodega in extraerBodega)
                                {
                                    int idBodega = Convert.ToInt32(bodega);
                                    List<ubicacion_repuesto> buscar = context.ubicacion_repuesto.Where(x => x.bodega == idBodega).ToList();
                                    if (buscar != null && buscar.Count != 0)
                                    {
                                        foreach (ubicacion_repuesto item in buscar)
                                        {
                                            //item.fisico = false;
                                            context.Entry(item).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }

                            context.SaveChanges();
                            TempData["mensaje"] = "El inventario ha sido desbloqueado correctamente";
                        }
                    }
                }
            }

            BuscarFavoritos(menu);
            //listas();
            return View();
        }

        #endregion

        //get
        public ActionResult ConteoInventario(int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int user_log = Convert.ToInt32(Session["user_usuarioid"]);
            if (user_log != 0)
            {
                var encab = context.aencab_auditoriainv.Where(x => x.bodega == bodega && x.estado == true)
                    .Select(x => new { x.id, x.tipo_conteo }).FirstOrDefault();
                if (encab != null)
                {
                    var buscar_p1 = context.apareja_auditoriainv.Where(x =>
                            x.id_encabezado == encab.id && (x.id_usuario_1 == user_log || x.id_usuario_2 == user_log))
                        .Select(x => new
                        {
                            nombre = x.users.user_nombre + " " + x.users.user_apellido,
                            id = x.users.user_id,
                            id_pareja = x.id,
                            x.finalizado
                        }).FirstOrDefault();

                    var buscar_p2 = context.apareja_auditoriainv.Where(x =>
                            x.id_encabezado == encab.id && (x.id_usuario_1 == user_log || x.id_usuario_2 == user_log))
                        .Select(x => new
                        {
                            noexiste = x.id_usuario_2 != null ? true : false,
                            nombre = x.id_usuario_2 != null ? x.users1.user_nombre + " " + x.users1.user_apellido : "",
                            id = x.id_usuario_2 != null ? x.users1.user_id : 0,
                            id_pareja = x.id,
                            x.finalizado
                        }).FirstOrDefault();

                    if (buscar_p1 != null || buscar_p2 != null)
                    {
                        ViewBag.person_1 = buscar_p1.nombre;
                        ViewBag.person_1id = buscar_p1.id;
                        ViewBag.usuarioLog = user_log;
                        ViewBag.person_2 = buscar_p2.nombre;
                        ViewBag.person_2id = buscar_p2.id;
                        ViewBag.person_2ne = buscar_p2.noexiste != true ? 0 : 1;
                        ViewBag.pareja_id = buscar_p1 != null ? buscar_p1.id_pareja : buscar_p2.id_pareja;
                        ViewBag.finalizado = buscar_p1.finalizado != true
                            ? Convert.ToInt32(buscar_p1.finalizado)
                            : Convert.ToInt32(buscar_p2.finalizado);
                    }
                    else
                    {
                        ViewBag.person_1 = "";
                        ViewBag.person_1id = 0;
                        ViewBag.usuarioLog = user_log;
                        ViewBag.person_2 = "";
                        ViewBag.person_2id = 0;
                        ViewBag.pareja_id = 0;
                        ViewBag.finalizado = 0;
                        ViewBag.person_2ne = false;
                    }

                    ViewBag.encabezado = encab.id;
                    ViewBag.tipo_conteo = encab.tipo_conteo;
                }
            }

            listas(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConteoInventario(InventarioModel inventario, int? menu)
        {
            int bodega = Convert.ToInt32(inventario.bodega_id);
            int area = Convert.ToInt32(inventario.area_id);
            int ubicacion = Convert.ToInt32(inventario.ubicacion_id);


            if (ModelState.IsValid)
            {
                var buscar = (from a in context.ubicacion_repuesto
                              where a.bodega == bodega && a.idarea == area && a.ubicacion == ubicacion &&
                                    a.codigo == inventario.referencia_id
                              select new
                              {
                                  a.id,
                                  a.toma_1,
                                  a.toma_2,
                                  a.toma_3
                              }).FirstOrDefault();
                if (buscar != null)
                {
                    if (buscar.toma_1 == null)
                    {
                        ubicacion_repuesto asignar = context.ubicacion_repuesto.FirstOrDefault(x => x.id == buscar.id);
                        asignar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        asignar.fec_actualizacion = DateTime.Now;
                        asignar.toma_1 = float.Parse(Request["txt_contador"]);
                        asignar.usu_toma_1 = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(asignar).State = EntityState.Modified;
                    }

                    if (buscar.toma_1 != null && buscar.toma_2 == null)
                    {
                        ubicacion_repuesto asignar = context.ubicacion_repuesto.FirstOrDefault(x => x.id == buscar.id);
                        asignar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        asignar.fec_actualizacion = DateTime.Now;
                        asignar.toma_2 = float.Parse(Request["txt_contador"]);
                        asignar.usu_toma_2 = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(asignar).State = EntityState.Modified;
                    }

                    if (buscar.toma_1 != null && buscar.toma_2 != null && buscar.toma_3 == null)
                    {
                        ubicacion_repuesto asignar = context.ubicacion_repuesto.FirstOrDefault(x => x.id == buscar.id);
                        asignar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        asignar.fec_actualizacion = DateTime.Now;
                        asignar.toma_3 = float.Parse(Request["txt_contador"]);
                        asignar.usu_toma_3 = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(asignar).State = EntityState.Modified;
                    }

                    int guardar = context.SaveChanges();
                    if (guardar == 1)
                    {
                        TempData["mensaje"] = "Conteo guardado";
                    }
                    else if (guardar == 0)
                    {
                        TempData["mensaje_error"] =
                            "Todos los conteos están llenos para esta referencia, no se pudo guardar";
                    }
                }
            }

            var buscarBodega = (from a in context.ubicacion_repuesto
                                join b in context.bodega_concesionario
                                    on a.bodega equals b.id
                                //where a.fisico == true
                                select new
                                {
                                    b.id,
                                    b.bodccs_nombre
                                }).Distinct().ToList();

            var buscarArea = (from a in context.ubicacion_repuesto
                              join b in context.area_bodega
                                  on a.idarea equals b.areabod_id
                              //where a.fisico == true
                              select new
                              {
                                  b.areabod_id,
                                  b.areabod_nombre
                              }).Distinct().ToList();

            var buscarUbicacion = (from a in context.ubicacion_repuesto
                                   join b in context.ubicacion_repuestobod
                                       on a.ubicacion equals b.id
                                   where /*a.fisico == true &&*/ a.idarea == area && a.bodega == bodega
                                   select new
                                   {
                                       b.id,
                                       b.descripcion
                                   }).Distinct().ToList();


            var buscarReferencias = (from a in context.ubicacion_repuesto
                                     join b in context.icb_referencia
                                         on a.codigo equals b.ref_codigo
                                     where /*a.fisico == true &&*/ a.ubicacion == ubicacion && a.idarea == area && a.bodega == bodega
                                     select new
                                     {
                                         b.ref_codigo,
                                         ref_descripcion = " ( " + b.ref_codigo + " ) - " + b.ref_descripcion
                                     }).Distinct().ToList();


            ViewBag.bodega_id = new SelectList(buscarBodega, "id", "bodccs_nombre", bodega);
            ViewBag.area_id = new SelectList(buscarArea, "areabod_id", "areabod_nombre", area);
            ViewBag.ubicacion_id = new SelectList(buscarUbicacion, "id", "descripcion", ubicacion);
            ViewBag.referencia_id =
                new SelectList(buscarReferencias, "ref_codigo", "ref_descripcion", inventario.referencia_id);
            return View();
        }

        public ActionResult asignacionParejas(int? id)
        {
            var usuarios = context.users.Where(x => x.user_estado).Select(x => new
            {
                value = x.user_id,
                text = "(" + x.rols.rol_nombre + ") " + x.user_nombre + " " + x.user_apellido
            }).OrderBy(x => x.text).ToList();
            ViewBag.usuarios_1 = new SelectList(usuarios, "value", "text");
            ViewBag.usuarios_2 = new SelectList(usuarios, "value", "text");
            ViewBag.id_encabezado = id;
            ViewBag.encabezado = context.aencab_auditoriainv.Where(x => x.id == id).Select(x => x.descripcion)
                .FirstOrDefault();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult asignacionParejas()
        {
            apareja_auditoriainv asignar = new apareja_auditoriainv();

            int contador = Convert.ToInt32(Request["contador"]);


            for (int i = 0; i < contador; i++)
            {
                string id_BD = Request["parejaId_bd" + i];
                string persona2 = Request["personas2_" + i];
                int? valida = Convert.ToInt32(Request["parejaId_" + i]);
                if (valida != 0 && (id_BD == null || id_BD == ""))
                {
                    asignar.cod_pareja = Convert.ToInt32(Request["parejaId_" + i]);
                    asignar.id_usuario_1 = Convert.ToInt32(Request["personas1_" + i]);
                    if (Request["personas2_" + i] != "")
                    {
                        asignar.id_usuario_2 = Convert.ToInt32(Request["personas2_" + i]);
                    }
                    else
                    {
                        asignar.id_usuario_2 = null;
                    }

                    asignar.fecha_creacion = DateTime.Now;
                    asignar.id_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    asignar.id_encabezado = Convert.ToInt32(Request["encabezado_id"]);
                    asignar.finalizado = false;
                    context.apareja_auditoriainv.Add(asignar);
                    context.SaveChanges();
                }
            }

            var usuarios = context.users.Where(x => x.user_estado).Select(x => new
            {
                value = x.user_id,
                text = "(" + x.rols.rol_nombre + ") " + x.user_nombre + " " + x.user_apellido
            }).OrderBy(x => x.text).ToList();


            int encabezado = Convert.ToInt32(Request["encabezado_id"]);

            aencab_auditoriainv buscar = context.aencab_auditoriainv.Where(x => x.id == encabezado).FirstOrDefault();


            ViewBag.usuarios_1 = new SelectList(usuarios, "value", "text");
            ViewBag.usuarios_2 = new SelectList(usuarios, "value", "text");
            ViewBag.id_encabezado = buscar.id;
            ViewBag.encabezado = buscar.descripcion;
            return RedirectToAction("edit", new { id = asignar.id_encabezado });
        }

        public ActionResult encabezado()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult encabezado(encabezadoModel modelo)
        {
            var buscarFechas = context.aencab_auditoriainv
                .Select(x => new { x.fecha_fin_aInv, x.fecha_inicio_aInv, x.bodega }).ToList();
            bool ok = true;
            foreach (var item in buscarFechas)
            {
                if (Convert.ToDateTime(modelo.fechaInicia) == item.fecha_fin_aInv ||
                    Convert.ToDateTime(modelo.fechaInicia) == item.fecha_inicio_aInv && modelo.bodegas == item.bodega)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaTermina) == item.fecha_fin_aInv ||
                    Convert.ToDateTime(modelo.fechaTermina) == item.fecha_inicio_aInv && modelo.bodegas == item.bodega)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaInicia) <= item.fecha_fin_aInv &&
                    Convert.ToDateTime(modelo.fechaInicia) >= item.fecha_inicio_aInv &&
                    modelo.bodegas == item.bodega)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaTermina) <= item.fecha_fin_aInv &&
                    Convert.ToDateTime(modelo.fechaTermina) >= item.fecha_inicio_aInv &&
                    modelo.bodegas == item.bodega)
                {
                    ok = false;
                }
            }

            if (ok)
            {
                string areas = string.Join(",", modelo.areas);
                string ubicaciones = string.Join(",", modelo.ubicaciones);
                string estanterias = string.Join(",", modelo.estanterias);

                aencab_auditoriainv nuevo = new aencab_auditoriainv
                {
                    descripcion = modelo.encabezado,
                    fecha_inicio_aInv = Convert.ToDateTime(modelo.fechaInicia),
                    fecha_fin_aInv = Convert.ToDateTime(modelo.fechaTermina).AddHours(23).AddMinutes(59).AddSeconds(59),
                    tipo_conteo =
                        Convert.ToInt32(modelo
                            .tipoConteo), //Si modelo.tipoConteo == 1 a las parejas se le deben asignar 2 conteos
                    fecha_creacion = DateTime.Now,
                    id_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    estado = false,
                    finalizado = false,
                    bodega = modelo.bodegas,
                    areas = areas,
                    estanterias = estanterias,
                    ubicaciones = ubicaciones
                };
                context.aencab_auditoriainv.Add(nuevo);
                context.SaveChanges();
                TempData["mensaje"] = "Encabezado creado correctamente";
                return View();
            }

            TempData["mensaje_error"] = "Ya existe un encabezado que se cruza con estas fechas en esta bodega";
            return View(modelo);
        }

        public ActionResult edit(int? menu, int? id)
        {
            aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(id);
            ViewBag.tipo = buscar.tipo_conteo;
            listas(id);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult edit(encabezadoModel modelo)
        {
            var buscarFechas = context.aencab_auditoriainv
                .Select(x => new { x.fecha_fin_aInv, x.fecha_inicio_aInv, x.bodega, x.id }).ToList();
            bool ok = true;
            foreach (var item in buscarFechas)
            {
                if (Convert.ToDateTime(modelo.fechaInicia) == item.fecha_fin_aInv ||
                    Convert.ToDateTime(modelo.fechaInicia) == item.fecha_inicio_aInv && modelo.bodegas == item.bodega &&
                    modelo.id != item.id)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaTermina) == item.fecha_fin_aInv ||
                    Convert.ToDateTime(modelo.fechaTermina) == item.fecha_inicio_aInv &&
                    modelo.bodegas == item.bodega && modelo.id != item.id)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaInicia) <= item.fecha_fin_aInv &&
                    Convert.ToDateTime(modelo.fechaInicia) >= item.fecha_inicio_aInv && modelo.bodegas == item.bodega &&
                    modelo.id != item.id)
                {
                    ok = false;
                }

                if (Convert.ToDateTime(modelo.fechaTermina) <= item.fecha_fin_aInv &&
                    Convert.ToDateTime(modelo.fechaTermina) >= item.fecha_inicio_aInv &&
                    modelo.bodegas == item.bodega && modelo.id != item.id)
                {
                    ok = false;
                }
            }

            if (ok)
            {
                string areas = string.Join(",", modelo.areas);
                string ubicaciones = string.Join(",", modelo.ubicaciones);
                string estanterias = string.Join(",", modelo.estanterias);
                aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(modelo.id);
                if (buscar != null)
                {
                    buscar.descripcion = modelo.encabezado;
                    //buscar.fecha_inicio_aInv = Convert.ToDateTime(modelo.fechaInicia);
                    //buscar.fecha_fin_aInv = Convert.ToDateTime(modelo.fechaTermina).AddHours(23).AddMinutes(59).AddSeconds(59);
                    buscar.tipo_conteo =
                        Convert.ToInt32(modelo
                            .tipoConteo); //Si modelo.tipoConteo == 1 a las parejas se le deben asignar 2 conteos
                    buscar.fecha_actualizacion = DateTime.Now;
                    buscar.id_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscar.estado = false;
                    buscar.bodega = modelo.bodegas;
                    buscar.areas = areas;
                    buscar.estanterias = estanterias;
                    buscar.ubicaciones = ubicaciones;
                }

                ;
                context.Entry(buscar).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "Encabezado actualizado correctamente";

                ViewBag.tipo = Convert.ToInt16(modelo.tipoConteo);
                listas(buscar.id);
                return View();
            }

            ViewBag.encabezado = modelo.encabezado;
            ViewBag.tipoConteo = Convert.ToString(modelo.tipoConteo);
            ViewBag.bodegas = new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado), "id",
                "bodccs_nombre", modelo.bodegas);
            ViewBag.areas = modelo.areas;
            ViewBag.ubicaciones = modelo.ubicaciones;
            ViewBag.fechaInicia = modelo.fechaInicia;
            ViewBag.fechaTermina = modelo.fechaTermina;
            TempData["mensaje_error"] = "Ya existe un encabezado que se cruza con estas fechas en esta bodega";
            listas(modelo.id);
            return View(modelo);
        }

        public ActionResult Informe(int id)
        {
            aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(id);
            ViewBag.id_encabezado = id;
            ViewBag.tipo = Convert.ToInt32(buscar.tipo_conteo);
            return View();
        }

        #endregion

        #region JsonResult

        public JsonResult browserEncabezado()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            var buscar = (from a in context.aencab_auditoriainv
                          join b in context.bodega_concesionario
                              on a.bodega equals b.id
                          where a.bodega == bodega
                          select new
                          {
                              a.descripcion,
                              a.fecha_inicio_aInv,
                              a.fecha_fin_aInv,
                              a.tipo_conteo,
                              b.bodccs_nombre,
                              id_bodega = b.id,
                              a.estado,
                              a.finalizado,
                              a.id,
                              cant_total = (from aa in context.aencab_auditoriainv
                                            join bb in context.vw_inventario_hoy
                                                on a.bodega equals bb.bodega into xx
                                            from bb in xx.DefaultIfEmpty()
                                                //where a.estado == true
                                            select new
                                            {
                                                num = bb.stock
                                            }).Distinct().ToList(),
                              can_punteada = (from aaa in context.aconteos_auditoria
                                              where a.id == aaa.encabezado
                                              select new
                                              {
                                                  aaa.conteo1,
                                                  aaa.conteo2
                                              }).ToList()
                          }).ToList();

            //var cantidad

            var data = buscar.Select(x => new
            {
                encabezado = x.descripcion,
                fechaInicia = x.fecha_inicio_aInv.ToString("yyyy/MM/dd"),
                fechaFinal = x.fecha_fin_aInv.ToString("yyyy/MM/dd"),
                tipo = x.tipo_conteo == 0 ? "1 Conteo" : "2 Conteos",
                bodega = x.bodccs_nombre,
                estado = x.finalizado == true ? "Finalizado" : x.estado == false ? "Inactivo" : "Activo",
                id_encabezado = x.id,
                cant_total = x.cant_total.Sum(a => a.num),
                progreso_1 = x.cant_total.Sum(a => a.num) > 0
                    ? Convert.ToInt32(x.can_punteada.Sum(s => s.conteo1) * 100 / x.cant_total.Sum(a => a.num))
                    : 0,
                progreso_2 = x.cant_total.Sum(a => a.num) > 0
                    ? Convert.ToInt32(x.can_punteada.Sum(s => s.conteo2) * 100 / x.cant_total.Sum(a => a.num))
                    : 0
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarBodegaDisponible()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            var buscarBodega = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).Select(x =>
                new
                {
                    x.bodccs_nombre,
                    x.id
                }).OrderBy(x => x.bodccs_nombre).ToList();


            return Json(buscarBodega, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAreaDisponible(string id)
        {
            int idbodega = Convert.ToInt32(id);

            var area = (from a in context.area_bodega
                        where a.id_bodega == idbodega
                        select new
                        {
                            a.areabod_id,
                            a.areabod_nombre
                        }).Distinct().ToList();

            return Json(area, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarUbicacion(int[] id)
        {
            List<SelectListItem> ubicaciones = new List<SelectListItem>();

            foreach (int item in id)
            {
                var ubicacion = (from a in context.ubicacion_repuestobod
                                 where a.id_estanteria == item
                                 select new
                                 {
                                     a.id,
                                     a.descripcion
                                 }).ToList();

                foreach (var a in ubicacion)
                {
                    ubicaciones.Add(new SelectListItem
                    {
                        Value = Convert.ToString(a.id),
                        Text = a.descripcion
                    });
                }
            }

            return Json(ubicaciones, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarStandDisponible(int[] id)
        {
            List<SelectListItem> estanteria = new List<SelectListItem>();

            foreach (int item in id)
            {
                var stand = (from a in context.estanterias
                             where a.id_area == item
                             select new
                             {
                                 a.id,
                                 a.descripcion
                             }).ToList();

                foreach (var a in stand)
                {
                    estanteria.Add(new SelectListItem
                    {
                        Value = Convert.ToString(a.id),
                        Text = a.descripcion
                    });
                }
            }

            return Json(estanteria, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarStandAsignado(int? id)
        {
            List<SelectListItem> estanteria = new List<SelectListItem>();
            aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(id);
            string[] estanterias = buscar.estanterias.Split(',');
            int n = 0;
            for (int i = 0; i < estanterias.Count(); i++)
            {
                n = Convert.ToInt32(estanterias[i]);
                var stand = (from a in context.estanterias
                             where a.id == n
                             select new
                             {
                                 a.id,
                                 a.descripcion
                             }).FirstOrDefault();


                estanteria.Add(new SelectListItem
                {
                    Value = Convert.ToString(stand.id),
                    Text = stand.descripcion
                });
            }

            return Json(estanteria, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarEstateria(int[] id)
        {
            List<SelectListItem> estanteria = new List<SelectListItem>();

            foreach (int item in id)
            {
                var stand = (from a in context.estanterias
                             where a.id_area == item
                             select new
                             {
                                 a.id,
                                 a.descripcion
                             }).ToList();

                foreach (var a in stand)
                {
                    estanteria.Add(new SelectListItem
                    {
                        Value = Convert.ToString(a.id),
                        Text = a.descripcion
                    });
                }
            }

            return Json(estanteria, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarBodegaBloqueada()
        {
            var area = (from a in context.ubicacion_repuesto
                        join b in context.bodega_concesionario
                            on a.bodega equals b.id
                        where b.bodccs_estado /*&& a.fisico == true*/
                        select new
                        {
                            b.id,
                            b.bodccs_nombre
                        }).Distinct().OrderBy(x => x.bodccs_nombre).ToList();


            return Json(area, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAreaBloqueada(string id)
        {
            int idbodega = Convert.ToInt32(id);

            var area = (from a in context.ubicacion_repuesto
                        join b in context.area_bodega
                            on a.idarea equals b.areabod_id
                        where /*a.fisico == true &&*/ a.bodega == idbodega
                        select new
                        {
                            b.areabod_id,
                            b.areabod_nombre
                        }).Distinct().ToList();


            return Json(area, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarUbicacionBloqueada(string id)
        {
            int idarea = Convert.ToInt32(id);

            var ubicacion = (from a in context.ubicacion_repuesto
                             join b in context.ubicacion_repuestobod
                                 on a.ubicacion equals b.id
                             where /*a.fisico == true &&*/ a.idarea == idarea
                             select new
                             {
                                 b.id,
                                 b.descripcion
                             }).Distinct().ToList();

            return Json(ubicacion, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarDatos(string bodega_id, string area_id, string ubicacion_id, string referencia_id)
        {
            #region conversorEntero

            int bodega = 0;
            int area = 0;
            int ubicacion = 0;

            if (bodega_id != "")
            {
                bodega = Convert.ToInt32(bodega_id);
            }

            if (area_id != "")
            {
                area = Convert.ToInt32(area_id);
            }

            if (ubicacion_id != "")
            {
                ubicacion = Convert.ToInt32(ubicacion_id);
            }

            #endregion

            #region cargarArea

            if (bodega != 0 && area == 0)
            {
                var buscarArea = (from a in context.ubicacion_repuesto
                                  join b in context.area_bodega
                                      on a.idarea equals b.areabod_id
                                  where /*a.fisico == true &&*/ a.bodega == bodega
                                  select new
                                  {
                                      b.areabod_id,
                                      b.areabod_nombre
                                  }).Distinct().ToList();

                ViewBag.area_id = new SelectList(buscarArea, "areabod_id", "areabod_nombre");

                var data = new
                {
                    ViewBag.area_id
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region cargarUbicacion

            if (area != 0 && ubicacion == 0)
            {
                var buscarUbicacion = (from a in context.ubicacion_repuesto
                                       join b in context.ubicacion_repuestobod
                                           on a.ubicacion equals b.id
                                       where /*a.fisico == true &&*/ a.idarea == area && a.bodega == bodega
                                       select new
                                       {
                                           b.id,
                                           b.descripcion
                                       }).Distinct().ToList();

                ViewBag.ubicacion_id = new SelectList(buscarUbicacion, "id", "descripcion");

                var data = new
                {
                    ViewBag.ubicacion_id
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region cargarReferencia

            if (ubicacion != 0 && referencia_id == "")
            {
                var buscarReferencias = (from a in context.ubicacion_repuesto
                                         join b in context.icb_referencia
                                             on a.codigo equals b.ref_codigo
                                         where /*a.fisico == true && */ a.ubicacion == ubicacion && a.idarea == area && a.bodega == bodega
                                         select new
                                         {
                                             b.ref_codigo,
                                             ref_descripcion = " ( " + b.ref_codigo + " ) - " + b.ref_descripcion
                                         }).Distinct().ToList();

                ViewBag.referencia_id = new SelectList(buscarReferencias, "ref_codigo", "ref_descripcion");

                var data = new
                {
                    ViewBag.referencia_id
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Contador(string id, string idPunteado)
        {
            if (id == idPunteado)
            {
                return Json(new { data = true }, JsonRequestBehavior.AllowGet);
            }

            if (idPunteado == "")
            {
                return Json(new { data = "vacio" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarConteo(string bodega_id, string area_id, string ubicacion_id, string referencia_id)
        {
            #region conversorEntero

            int bodega = 0;
            int area = 0;
            int ubicacion = 0;

            if (bodega_id != "")
            {
                bodega = Convert.ToInt32(bodega_id);
            }

            if (area_id != "")
            {
                area = Convert.ToInt32(area_id);
            }

            if (ubicacion_id != "")
            {
                ubicacion = Convert.ToInt32(ubicacion_id);
            }

            #endregion

            var buscar = (from a in context.ubicacion_repuesto
                          where a.bodega == bodega && a.idarea == area && a.ubicacion == ubicacion && a.codigo == referencia_id
                          select new
                          {
                              a.toma_1,
                              a.toma_2,
                              a.toma_3
                          }).ToList();

            var data = new
            {
                buscar
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BorrarUltimoContador(string bodega_id, string area_id, string ubicacion_id,
            string referencia_id, int? conteo)
        {
            #region conversorEntero

            int bodega = 0;
            int area = 0;
            int ubicacion = 0;

            if (bodega_id != "")
            {
                bodega = Convert.ToInt32(bodega_id);
            }

            if (area_id != "")
            {
                area = Convert.ToInt32(area_id);
            }

            if (ubicacion_id != "")
            {
                ubicacion = Convert.ToInt32(ubicacion_id);
            }

            #endregion

            var buscar = (from a in context.ubicacion_repuesto
                          where a.bodega == bodega && a.idarea == area && a.ubicacion == ubicacion && a.codigo == referencia_id
                          select new
                          {
                              a.id,
                              a.toma_1,
                              a.toma_2,
                              a.toma_3
                          }).FirstOrDefault();

            ubicacion_repuesto cambio = context.ubicacion_repuesto.FirstOrDefault(x => x.id == buscar.id);

            if (conteo == 1)
            {
                cambio.toma_1 = null;
                cambio.usu_toma_1 = null;
                cambio.fec_actualizacion = DateTime.Now;
                cambio.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            }

            if (conteo == 2)
            {
                cambio.toma_2 = null;
                cambio.usu_toma_2 = null;
                cambio.fec_actualizacion = DateTime.Now;
                cambio.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            }

            if (conteo == 3)
            {
                cambio.toma_3 = null;
                cambio.usu_toma_3 = null;
                cambio.fec_actualizacion = DateTime.Now;
                cambio.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            }

            context.Entry(cambio).State = EntityState.Modified;
            int guardar = context.SaveChanges();
            if (guardar > 0)
            {
                return Json(new { data = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermisoModificarValores()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permisoBloquear = (from u in context.users
                                   join r in context.rols
                                       on u.rol_id equals r.rol_id
                                   join ra in context.rolacceso
                                       on r.rol_id equals ra.idrol
                                   where u.user_id == usuario && ra.idpermiso == 18
                                   select new
                                   {
                                       u.user_id,
                                       u.rol_id,
                                       r.rol_nombre,
                                       ra.idpermiso
                                   }).Count();

            int permisoDesloquear = (from u in context.users
                                     join r in context.rols
                                         on u.rol_id equals r.rol_id
                                     join ra in context.rolacceso
                                         on r.rol_id equals ra.idrol
                                     where u.user_id == usuario && ra.idpermiso == 19
                                     select new
                                     {
                                         u.user_id,
                                         u.rol_id,
                                         r.rol_nombre,
                                         ra.idpermiso
                                     }).Count();

            var data = new
            {
                permisoBloquear,
                permisoDesloquear
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult refrescarParejas(int? usuario_1, int? usuario_2, bool act_lista_1, bool act_lista_2,
            int?[] tabla)
        {
            var usuarios = context.users.Where(x => x.user_estado).Select(x => new
            {
                value = x.user_id,
                text = "(" + x.rols.rol_nombre + ") " + x.user_nombre + " " + x.user_apellido
            }).OrderBy(x => x.text).ToList();

            if (tabla != null)
            {
                foreach (int? item in tabla)
                {
                    usuarios = usuarios.Where(x => x.value != item).ToList();
                }
            }

            if (act_lista_1)
            {
                usuarios = usuarios.Where(x => x.value != usuario_2).ToList();
            }

            if (act_lista_2)
            {
                usuarios = usuarios.Where(x => x.value != usuario_1).ToList();
            }

            return Json(new { act_lista_1, act_lista_2, usuarios }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarParejas(int? id)
        {
            var buscar = (from a in context.apareja_auditoriainv
                          join b in context.users
                              on a.id_usuario_1 equals b.user_id
                          join c in context.users
                              on a.id_usuario_2 equals c.user_id into xx
                          from c in xx.DefaultIfEmpty()
                          where a.id_encabezado == id
                          select new
                          {
                              numero = a.cod_pareja,
                              id_pareja = a.id,
                              persona_1 = "(" + b.rols.rol_nombre + ") " + b.user_nombre + " " + b.user_apellido,
                              id_persona_1 = b.user_id,
                              persona_2 = a.id_usuario_2 != null
                                  ? "(" + c.rols.rol_nombre + ") " + c.user_nombre + " " + c.user_apellido
                                  : "",
                              id_persona_2 = a.id_usuario_2 != null ? c.user_id : 0,
                              ubicaciones = (from ubi in context.ubicaciones_asignadas
                                             join name in context.ubicacion_repuestobod
                                                 on ubi.id_ubicacion equals name.id into zz
                                             from name in zz.DefaultIfEmpty()
                                             where ubi.id_pareja == a.id
                                             select name.descripcion
                                  ).ToList(),
                              ubicaciones2 = (from ubi in context.ubicaciones_asignadas_2
                                              join name in context.ubicacion_repuestobod
                                                  on ubi.id_ubicacion equals name.id into zz
                                              from name in zz.DefaultIfEmpty()
                                              where ubi.id_pareja == a.id
                                              select name.descripcion
                                  ).ToList()
                          }).ToList();

            List<parejas_ubicaciones> nuevo = new List<parejas_ubicaciones>();

            foreach (var item in buscar)
            {
                nuevo.Add(new parejas_ubicaciones
                {
                    numero = item.numero,
                    id_pareja = item.id_pareja,
                    persona_1 = item.persona_1,
                    id_persona_1 = item.id_persona_1,
                    persona_2 = item.persona_2,
                    id_persona_2 = item.id_persona_2,
                    ubicaciones = string.Join(" - ", item.ubicaciones),
                    ubicaciones2 = string.Join(" - ", item.ubicaciones2)
                });
            }

            return Json(nuevo, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarParejas(int? id)
        {
            apareja_auditoriainv buscar = context.apareja_auditoriainv.Where(x => x.id == id).FirstOrDefault();

            if (buscar != null)
            {
                List<int> buscarUbicacionesAsignadas = context.ubicaciones_asignadas.Where(x => x.id_pareja == id)
                    .Select(x => x.id).ToList();
                List<int> buscarUbicacionesAsignadas2 = context.ubicaciones_asignadas_2.Where(x => x.id_pareja == id)
                    .Select(x => x.id).ToList();

                if (buscarUbicacionesAsignadas.Count > 0)
                {
                    foreach (int item in buscarUbicacionesAsignadas)
                    {
                        ubicaciones_asignadas buscarUbicaciones = context.ubicaciones_asignadas.Find(item);
                        context.Entry(buscarUbicaciones).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                }

                if (buscarUbicacionesAsignadas2.Count > 0)
                {
                    foreach (int item in buscarUbicacionesAsignadas2)
                    {
                        ubicaciones_asignadas_2 buscarUbicaciones = context.ubicaciones_asignadas_2.Find(item);
                        context.Entry(buscarUbicaciones).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                }

                context.Entry(buscar).State = EntityState.Deleted;
            }

            context.SaveChanges();

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traer_ubicaciones(int? id_encab, int? id_pareja, int tipo)
        {
            if (tipo == 0)
            {
               
                var asignadas = context.ubicaciones_asignadas.Where(x => x.id_encab == id_encab && x.id_pareja == id_pareja)
                    .Select( x => new{ x.id_ubicacion,  x.ubicacion_repuestobod.descripcion}).ToList();

      

                return Json(asignadas, JsonRequestBehavior.AllowGet);
            }

            if (tipo == 1)
            {
              
                var  asignadas = context.ubicaciones_asignadas_2.Where(x => x.id_encab == id_encab && x.id_pareja == id_pareja)
                    .Select(x => new { x.id_ubicacion, x.ubicacion_repuestobod.descripcion }).ToList();

       
                return Json(asignadas, JsonRequestBehavior.AllowGet);
            }
            return Json(0, JsonRequestBehavior.AllowGet);
            }

        public JsonResult asignarUbicacionGrupo(int? grupo, int?[] ubicaciones, int? encab, int? tipo)
        {
            if (tipo == 0)
            {
                ubicaciones_asignadas buscar = context.ubicaciones_asignadas.Find(grupo);

                ubicaciones_asignadas nuevo = new ubicaciones_asignadas();
                foreach (int? item in ubicaciones)
                {
                    nuevo.id_pareja = grupo;
                    nuevo.id_ubicacion = item;
                    nuevo.id_encab = encab;
                    context.ubicaciones_asignadas.Add(nuevo);
                    context.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }

            if (tipo == 1)
            {
                ubicaciones_asignadas buscar = context.ubicaciones_asignadas.Find(grupo);

                ubicaciones_asignadas_2 nuevo = new ubicaciones_asignadas_2();
                foreach (int? item in ubicaciones)
                {
                    nuevo.id_pareja = Convert.ToInt32(grupo);
                    nuevo.id_ubicacion = Convert.ToInt32(item);
                    nuevo.id_encab = Convert.ToInt32(encab);
                    context.ubicaciones_asignadas_2.Add(nuevo);
                    context.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult bloquearInventario(int? encab)
        {
            var buscarActivos = context.aencab_auditoriainv.Where(x => x.estado == true).Select(x => new
            {
                x.bodega
            }).ToList();

            aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(encab);

            bool bandera = false;

            foreach (var item in buscarActivos)
            {
                if (item.bodega == buscar.bodega)
                {
                    bandera = true;
                }
            }

            if (buscar.finalizado == true)
            {
                return Json(new { finalizado = true }, JsonRequestBehavior.AllowGet);
            }

            if (bandera == false)
            {
                string[] ubi = buscar.ubicaciones.Split(',');
                buscar.estado = true;
                context.Entry(buscar).State = EntityState.Modified;

                foreach (string item in ubi)
                {
                    int id = Convert.ToInt32(item);

                    ubicacion_repuestobod block = context.ubicacion_repuestobod.Where(x => x.id == id).FirstOrDefault();
                    block.fisico = true;
                    context.Entry(block).State = EntityState.Modified;
                    context.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult desbloquearInventario(int? encab)
        {
            aencab_auditoriainv buscar = context.aencab_auditoriainv.Find(encab);

            string[] ubi = buscar.ubicaciones.Split(',');
            buscar.estado = false;
            buscar.finalizado = true;
            context.Entry(buscar).State = EntityState.Modified;

            foreach (string item in ubi)
            {
                int id = Convert.ToInt32(item);

                ubicacion_repuestobod block = context.ubicacion_repuestobod.Where(x => x.id == id).FirstOrDefault();
                block.fisico = false;
                context.Entry(block).State = EntityState.Modified;
                context.SaveChanges();
            }

            return Json(0);
        }

        public JsonResult buscarPermiso()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            aencab_auditoriainv buscarEncab = context.aencab_auditoriainv.FirstOrDefault(x => x.estado == true && x.bodega == bodega);

            if (buscarEncab != null)
            {
                int buscarParejas = context.apareja_auditoriainv.Where(x =>
                        x.id_encabezado == buscarEncab.id && (x.id_usuario_1 == usuario || x.id_usuario_2 == usuario))
                    .Count();
                if (buscarParejas == 0)
                {
                    return Json(
                        new { estado = false, mensaje = "Usted no fue seleccionado para los conteos de este inventario" },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new { estado = true, mensaje = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { estado = false, mensaje = "No hay inventario activo para el día de hoy" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult autenticaciones(string contrasena, int? usuario1, int? usuario2, string existe)
        {
            bool ok = false;
            string mensaje = "";
            string buscar = context.users.Where(x => x.user_id == usuario1 || x.user_id == usuario2)
                .Select(x => x.user_password).FirstOrDefault();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(contrasena));
            byte[] result = md5.Hash;
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                str.Append(result[i].ToString("x2"));
            }

            string pass = str.ToString();
            if (pass == buscar)
            {
                ok = true;
                mensaje = "Autenticación correcta";
            }
            else
            {
                ok = false;
                mensaje = "Autenticación incorrecta, escriba su contraseña nuevamente";
            }

            if ((usuario2 == null || usuario2 == 0) && existe == "0")
            {
                ok = true;
                mensaje = "Autenticación correcta";
            }

            return Json(new { ok, mensaje }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult desicion(string codigo, int? encab, int tipo, string ubicacion)
        {
            codigo = codigo.Trim();
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            var refe = context.icb_referencia.Where(x => x.ref_codigo == codigo)
                .Select(x => new { codigo = "( " + x.ref_codigo + " ) " + x.ref_descripcion }).FirstOrDefault();
            string ubi = context.ubicacion_repuestobod
                .Where(x => x.descripcion == codigo && x.estanterias.area_bodega.bodega_concesionario.id == bodega)
                .Select(x => x.descripcion).FirstOrDefault();
            bool r = false;
            bool u = false;
            bool asignada = false;
            bool ref_asignada = false;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            if (tipo == 0)
            {
                int buscarEncab = context.apareja_auditoriainv
                    .Where(x => x.id_encabezado == encab && (x.id_usuario_1 == usuario || x.id_usuario_2 == usuario))
                    .Select(x => x.id).FirstOrDefault();
                List<string> buscarUbicaciones = context.ubicaciones_asignadas.Where(x => x.id_pareja == buscarEncab)
                    .Select(x => x.ubicacion_repuestobod.descripcion).ToList();
                foreach (string item in buscarUbicaciones)
                {
                    if (ubi != null)
                    {
                        if (item == codigo.ToUpper())
                        {
                            asignada = true;
                            break;
                        }
                    }
                }
            }

            if (tipo == 1)
            {
                int buscarEncab = context.apareja_auditoriainv
                    .Where(x => x.id_encabezado == encab && (x.id_usuario_1 == usuario || x.id_usuario_2 == usuario))
                    .Select(x => x.id).FirstOrDefault();
                List<string> buscarUbicaciones = context.ubicaciones_asignadas_2.Where(x => x.id_pareja == buscarEncab)
                    .Select(x => x.ubicacion_repuestobod.descripcion).ToList();
                foreach (string item in buscarUbicaciones)
                {
                    if (ubi != null)
                    {
                        if (item == codigo.ToUpper())
                        {
                            asignada = true;
                            break;
                        }
                    }
                }
            }

            if (tipo == 3)
            {
                int buscarEncab = context.apareja_auditoriainv
                    .Where(x => x.id_encabezado == encab && (x.id_usuario_1 == usuario || x.id_usuario_2 == usuario))
                    .Select(x => x.id).FirstOrDefault();
                List<string> buscarUbicaciones = context.ubicaciones_asignadas_3.Where(x => x.id_pareja == buscarEncab)
                    .Select(x => x.ubicacion_repuestobod.descripcion).ToList();
                foreach (string item in buscarUbicaciones)
                {
                    if (ubi != null)
                    {
                        if (item == codigo.ToUpper())
                        {
                            asignada = true;
                            break;
                        }
                    }
                }

                if (ubicacion != "")
                {
                    int buscarUbicacion = context.ubicacion_repuestobod
                        .Where(x => x.descripcion == ubicacion &&
                                    x.estanterias.area_bodega.bodega_concesionario.id == bodega).Select(x => x.id)
                        .FirstOrDefault();
                    List<string> buscarRef = context.ubicaciones_asignadas_3
                        .Where(x => x.id_pareja == buscarEncab && x.id_ubicacion == buscarUbicacion)
                        .Select(x => x.ref_codigo).ToList();
                    foreach (string item in buscarRef)
                    {
                        if (buscarRef != null)
                        {
                            if (item == codigo.ToUpper())
                            {
                                ref_asignada = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (refe != null)
            {
                r = true;
            }

            if (ubi != null)
            {
                u = true;
            }

            return Json(new { r, u, asignada, refe, ref_asignada }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Guardado(string refe, int ubi, int cont, int encab, int pareja, int tipo)
        {
            //Si tipo llega 0 es por que es el conteo 1
            //si tipo llega 1 es por que es el conteo 2
            aencab_auditoriainv buscarDatos = context.aencab_auditoriainv.Find(encab);
            int? bodega = buscarDatos.bodega;
            int existencia = context.vw_inventario_hoy.Where(x => x.bodega == bodega && x.ref_codigo == refe).Count();
            decimal stock = 0;
            if (existencia > 0)
            {
                stock = context.vw_inventario_hoy.FirstOrDefault(x => x.bodega == bodega && x.ref_codigo == refe).stock;
            }

            int coincidir = context.aconteos_auditoria
                .Where(x => x.id_referencia == refe && x.pareja == pareja && x.encabezado == encab && x.id == ubi)
                .Select(x => x.id).FirstOrDefault();

            coincidir = Convert.ToInt32(coincidir);


            if (tipo == 0 && coincidir == 0)
            {
                aconteos_auditoria nuevo = new aconteos_auditoria
                {
                    id_referencia = refe,
                    stock = Convert.ToInt32(stock),
                    conteo1 = cont,
                    pareja = pareja,
                    encabezado = encab,
                    ubicacion = ubi
                };
                context.aconteos_auditoria.Add(nuevo);
            }

            if (tipo == 1 && coincidir == 0)
            {
                aconteos_auditoria nuevo = new aconteos_auditoria
                {
                    id_referencia = refe,
                    stock = Convert.ToInt32(stock),
                    conteo2 = cont,
                    pareja = pareja,
                    encabezado = encab,
                    ubicacion = ubi
                };
                context.aconteos_auditoria.Add(nuevo);
            }

            if (tipo == 0 && coincidir > 0)
            {
                aconteos_auditoria editar = context.aconteos_auditoria.Find(coincidir);
                editar.conteo1 = Convert.ToInt32(editar.conteo1) + cont;
                context.Entry(editar).State = EntityState.Modified;
            }

            if (tipo == 1 && coincidir > 0)
            {
                aconteos_auditoria editar = context.aconteos_auditoria.Find(coincidir);
                editar.conteo2 = Convert.ToInt32(editar.conteo2) + cont;
                context.Entry(editar).State = EntityState.Modified;
            }

            if (tipo == 3)
            {
                int coincidir_2 = context.aconteos_auditoria
                    .Where(x => x.id_referencia == refe && x.encabezado == encab && x.id == ubi).Select(x => x.id)
                    .FirstOrDefault();
                aconteos_auditoria editar = context.aconteos_auditoria.Find(coincidir_2);
                editar.conteo3 = Convert.ToInt32(editar.conteo3) + cont;
                context.Entry(editar).State = EntityState.Modified;
            }

            context.SaveChanges();

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarUbicacionesAsignadas(int pareja)
        {
            var ubi1 = context.ubicaciones_asignadas.Where(x => x.id_pareja == pareja).Select(x => new
            {
                area = x.ubicacion_repuestobod.estanterias.area_bodega.areabod_nombre,
                estanteria = x.ubicacion_repuestobod.estanterias.descripcion,
                ubicacion = x.ubicacion_repuestobod.descripcion
            }).ToList();
            var ubi2 = context.ubicaciones_asignadas_2.Where(x => x.id_pareja == pareja).Select(x => new
            {
                area = x.ubicacion_repuestobod.estanterias.area_bodega.areabod_nombre,
                estanteria = x.ubicacion_repuestobod.estanterias.descripcion,
                ubicacion = x.ubicacion_repuestobod.descripcion
            }).ToList();


            return Json(new { ubi1, ubi2 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRegistros(int encab, int pareja, int tipo)
        {
            if (tipo == 3)
            {
                List<string> buscarReferencias = context.ubicaciones_asignadas_3
                    .Where(x => x.id_encab == encab && x.id_pareja == pareja).Select(x => x.ref_codigo).ToList();
                List<int> buscarUbicaciones = context.ubicaciones_asignadas_3
                    .Where(x => x.id_encab == encab && x.id_pareja == pareja).Select(x => x.id_ubicacion).ToList();

                var buscarConteo = context.aconteos_auditoria.Where(x =>
                    x.encabezado == encab && x.pareja == pareja && buscarReferencias.Contains(x.id_referencia) &&
                    buscarUbicaciones.Contains(x.ubicacion)).Select(x => new
                    {
                        encabezado = x.aencab_auditoriainv.descripcion,
                        bodega = x.aencab_auditoriainv.bodega_concesionario.bodccs_nombre,
                        grupo = x.apareja_auditoriainv.cod_pareja,
                        codigo = x.id_referencia,
                        descripcion = x.icb_referencia.ref_descripcion,
                        ubicacion = x.ubicacion_repuestobod.descripcion,
                        conteoExtra = x.conteo3 != null ? x.conteo3.ToString() : "Sin realizar"
                    }).ToList();

                return Json(buscarConteo, JsonRequestBehavior.AllowGet);
            }

            var buscar = context.aconteos_auditoria.Where(x => x.encabezado == encab && x.pareja == pareja).Select(x =>
                new
                {
                    x.id,
                    encabezado = x.aencab_auditoriainv.descripcion,
                    bodega = x.aencab_auditoriainv.bodega_concesionario.bodccs_nombre,
                    grupo = x.apareja_auditoriainv.cod_pareja,
                    referencia = x.icb_referencia.ref_codigo + " " + x.icb_referencia.ref_descripcion,
                    conteo = x.conteo1 != null ? 1 : x.conteo2 != null ? 2 : 3,
                    cantidad_1 = x.conteo1 != null ? x.conteo1.ToString() : "",
                    cantidad_2 = x.conteo2 != null ? x.conteo2.ToString() : "",
                    cantidad_3 = x.conteo3 != null ? x.conteo3.ToString() : "",
                    ubicacion = x.ubicacion_repuestobod.descripcion
                }).OrderByDescending(x => x.id).ToList().Take(10);

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult finalizarConteo(int pareja, bool act)
        {
            apareja_auditoriainv buscar = context.apareja_auditoriainv.Find(pareja);
            if (buscar != null)
            {
                if (buscar.finalizado == false)
                {
                    buscar.finalizado = act;
                    context.Entry(buscar).State = EntityState.Modified;
                }

                if (buscar.finalizado && buscar.conteo_3 && buscar.fin_conteo_3 == false)
                {
                    buscar.fin_conteo_3 = true;
                    context.Entry(buscar).State = EntityState.Modified;
                }

                context.SaveChanges();
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarListaPareja(int id_encab)
        {
            var cargar2 = context.apareja_auditoriainv.Where(x => x.id_encabezado == id_encab).Select(x => new
            {
                x.id,
                x.cod_pareja,
                persona_1 = x.users.user_nombre + " " + x.users.user_apellido,
                persona_2 = x.id_usuario_2 != null ? " - " + x.users1.user_nombre + " " + x.users1.user_apellido : ""
            }).ToList();
            var cargar = cargar2.Select(x => new
            {
                value = x.id,
                text = x.cod_pareja + " - " + x.persona_1 + "" + x.persona_2
            });

            return Json(cargar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarTablaDatos(int? id)
        {
            if (id != null)
            {
                var buscar = context.aconteos_auditoria.Where(x => x.pareja == id).Select(x => new
                {
                    x.id,
                    ubicacion = x.ubicacion_repuestobod.descripcion,
                    conteo_1 = x.conteo1 != null ? x.conteo1 : 0,
                    conteo_2 = x.conteo2 != null ? x.conteo2 : 0,
                    referencia = x.icb_referencia.ref_codigo + " " + x.icb_referencia.ref_descripcion,
                    ref_cod = x.id_referencia
                }).ToList();

                var buscarExtras = context.ubicaciones_asignadas_3.Where(x => x.id_pareja == id).Select(x => new
                {
                    x.id_ubicacion,
                    x.ref_codigo,
                    x.id_encab
                }).ToList();

                List<conteosExtra> extra = new List<conteosExtra>();
                foreach (var item in buscarExtras)
                {
                    var buscar2 = context.aconteos_auditoria
                        .Where(x => x.encabezado == item.id_encab && x.ubicacion == item.id_ubicacion &&
                                    x.id_referencia == item.ref_codigo).GroupBy(x => new { x.id_referencia, x.ubicacion })
                        .Select(grp => new
                        {
                            ubicacion = grp.Select(c => c.ubicacion_repuestobod.descripcion).FirstOrDefault(),
                            id_ubicacion = grp.Select(c => c.ubicacion).FirstOrDefault(),
                            ref_codigo = grp.Select(c => c.id_referencia).FirstOrDefault(),
                            descripcion = grp.Select(c => c.icb_referencia.ref_descripcion).FirstOrDefault(),
                            id_pareja = grp.Select(c => c.pareja).FirstOrDefault(),
                            conteo_3 = grp.Select(c => c.conteo3).Sum() != null ? grp.Select(c => c.conteo3).Sum() : 0,
                            id_conteo = grp.Select(c => c.id).FirstOrDefault()
                        }).FirstOrDefault();

                    extra.Add(new conteosExtra
                    {
                        id_ubicacion = Convert.ToInt32(buscar2.id_ubicacion),
                        ubicacion = buscar2.ubicacion,
                        ref_codigo = buscar2.ref_codigo,
                        referencia = buscar2.descripcion,
                        id_pareja = buscar2.id_pareja,
                        conteo3 = Convert.ToInt32(buscar2.conteo_3),
                        id_conteo = buscar2.id_conteo
                    });
                }

                var data = new
                {
                    buscar,
                    extra
                };


                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarCambios(int id, int conteo_1_nuevo, int conteo_1_antiguo, int conteo_2_nuevo,
            int conteo_2_antiguo)
        {
            int guardado = 0;
            if (conteo_1_antiguo > 0)
            {
                aconteos_auditoria buscar = context.aconteos_auditoria.Find(id);
                if (buscar.conteo1 != conteo_1_nuevo)
                {
                    buscar.conteo1 = conteo_1_nuevo;
                    context.Entry(buscar).State = EntityState.Modified;

                    amodificaciones_auditoriainv nuevo = new amodificaciones_auditoriainv
                    {
                        id_conteo = id,
                        conteo_1_anterior = conteo_1_antiguo,
                        conteo_1_nuevo = conteo_1_nuevo,
                        usuario_modifica = Convert.ToInt32(Session["user_usuarioid"]),
                        fecha_modifica = DateTime.Now
                    };
                    context.amodificaciones_auditoriainv.Add(nuevo);

                    guardado = context.SaveChanges();
                }
            }

            if (conteo_2_antiguo > 0)
            {
                aconteos_auditoria buscar = context.aconteos_auditoria.Find(id);
                if (buscar.conteo2 != conteo_2_nuevo)
                {
                    buscar.conteo2 = conteo_2_nuevo;
                    context.Entry(buscar).State = EntityState.Modified;

                    amodificaciones_auditoriainv nuevo = new amodificaciones_auditoriainv
                    {
                        id_conteo = id,
                        conteo_2_anterior = conteo_2_antiguo,
                        conteo_2_nuevo = conteo_2_nuevo,
                        usuario_modifica = Convert.ToInt32(Session["user_usuarioid"]),
                        fecha_modifica = DateTime.Now
                    };
                    context.amodificaciones_auditoriainv.Add(nuevo);

                    guardado = context.SaveChanges();
                }
            }

            if (guardado == 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarCambios2(int id, int conteo_3_nuevo, int conteo_3_antiguo, int pareja)
        {
            int guardado = 0;

            aconteos_auditoria buscar = context.aconteos_auditoria.Find(id);
            if (buscar.conteo3 != conteo_3_nuevo)
            {
                buscar.conteo3 = conteo_3_nuevo;
                context.Entry(buscar).State = EntityState.Modified;

                amodificaciones_extra_auditoriainv nuevo = new amodificaciones_extra_auditoriainv
                {
                    id_conteo = id,
                    conteo_extra_anterior = conteo_3_antiguo,
                    conteo_extra_nuevo = conteo_3_nuevo,
                    usuario_modifica = Convert.ToInt32(Session["user_usuarioid"]),
                    fecha_modifica = DateTime.Now,
                    pareja_solicita = pareja
                };
                context.amodificaciones_extra_auditoriainv.Add(nuevo);

                guardado = context.SaveChanges();
            }

            if (guardado == 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarDatosInforme(int id_encabezado, int? boton)
        {
            var par_fin = context.apareja_auditoriainv.Where(x => x.id_encabezado == id_encabezado)
                .Select(x => new { x.id, x.finalizado })
                .ToList(); //Busco que todas las parejas ya hayan terminado el conteo para calcular diferencias
            bool finalizado_ok = true;

            foreach (var item in par_fin)
            {
                if (item.finalizado == false)
                {
                    finalizado_ok = false;
                }
            }

            int bod = Convert.ToInt32(Session["user_bodega"]);
            var datos = (from a in context.aconteos_auditoria
                         join b in context.vw_inventario_hoy
                             on new { x1 = a.id_referencia, x2 = bod } equals new { x1 = b.ref_codigo, x2 = b.bodega } into xx
                         from b in xx.DefaultIfEmpty()
                         where a.encabezado == id_encabezado
                         select new
                         {
                             refer = a.icb_referencia.ref_codigo + " - " + a.icb_referencia.ref_descripcion,
                             a.id_referencia,
                             a.ubicacion,
                             a.ubicacion_repuestobod,
                             stock = b != null ? b.stock : 0,
                             a.conteo1,
                             a.conteo2,
                             a.conteo3
                         }).ToList();

            string encabezado = context.aencab_auditoriainv.FirstOrDefault(x => x.id == id_encabezado).descripcion;
            int tipo_encab = context.aencab_auditoriainv.FirstOrDefault(x => x.id == id_encabezado).tipo_conteo;
            var datos2 = datos.GroupBy(x => x.id_referencia).Select(grp => new
            {
                id_referencia = grp.Select(x => x.id_referencia).FirstOrDefault(),
                referencia = grp.Select(x => x.refer).FirstOrDefault(),
                ubicaciones = string.Join(" , ", grp.Select(x => x.ubicacion_repuestobod.descripcion).Distinct()),
                ubicaciones_id = string.Join(",", grp.Select(x => x.ubicacion_repuestobod.id).Distinct()),
                conteo1 = grp.Select(x => x.conteo1).Sum(),
                conteo2 = Convert.ToString(grp.Select(x => x.conteo2).Sum()),
                conteo3 = grp.Select(x => x.conteo3).Sum() != 0
                    ? Convert.ToString(grp.Select(x => x.conteo3).Sum())
                    : "",
                diferencias = "",
                stock = grp.Select(x => x.stock).FirstOrDefault()
            }).ToList();

            if (finalizado_ok)
            {
                if (tipo_encab == 0)
                {
                    datos2 = datos.GroupBy(x => x.id_referencia).Select(grp => new
                    {
                        id_referencia = grp.Select(x => x.id_referencia).FirstOrDefault(),
                        referencia = grp.Select(x => x.refer).FirstOrDefault(),
                        ubicaciones = string.Join(" , ",
                            grp.Select(x => x.ubicacion_repuestobod.descripcion).Distinct()),
                        ubicaciones_id = string.Join(",", grp.Select(x => x.ubicacion_repuestobod.id).Distinct()),
                        conteo1 = grp.Select(x => x.conteo1).Sum(),
                        conteo2 = "No aplica",
                        conteo3 = grp.Select(x => x.conteo3).Sum() != 0
                            ? Convert.ToString(grp.Select(x => x.conteo3).Sum())
                            : "",
                        diferencias =
                            Convert.ToString(Convert.ToInt32(
                                grp.Select(x => x.conteo1).Sum() - grp.Select(x => x.stock).FirstOrDefault())),
                        stock = grp.Select(x => x.stock).FirstOrDefault()
                    }).ToList();
                }

                if (tipo_encab == 1)
                {
                    datos2 = datos.GroupBy(x => x.id_referencia).Select(grp => new
                    {
                        id_referencia = grp.Select(x => x.id_referencia).FirstOrDefault(),
                        referencia = grp.Select(x => x.refer).FirstOrDefault(),
                        ubicaciones = string.Join(" , ",
                            grp.Select(x => x.ubicacion_repuestobod.descripcion).Distinct()),
                        ubicaciones_id = string.Join(",", grp.Select(x => x.ubicacion_repuestobod.id).Distinct()),
                        conteo1 = grp.Select(x => x.conteo1).Sum(),
                        conteo2 = Convert.ToString(grp.Select(x => x.conteo2).Sum()),
                        conteo3 = grp.Select(x => x.conteo3).Sum() != 0
                            ? Convert.ToString(grp.Select(x => x.conteo3).Sum())
                            : "",
                        diferencias =
                            Convert.ToString(grp.Select(x => x.conteo1).Sum() - grp.Select(x => x.conteo2).Sum()),
                        stock = grp.Select(x => x.stock).FirstOrDefault()
                    }).ToList();
                }
            }

            var data = new
            {
                datos2,
                encabezado
            };
            if (boton != null)
            {
                if (finalizado_ok != true)
                {
                    return Json(new { data, mensaje = 0 }, JsonRequestBehavior.AllowGet); // 0 es == a false
                }

                return Json(new { data, mensaje = 1 }, JsonRequestBehavior.AllowGet); //1 == a true
            }

            if (finalizado_ok != true)
            {
                return Json(new { data, mensaje = 0 }, JsonRequestBehavior.AllowGet); // 0 es == a false
            }

            return Json(new { data, mensaje = 1 }, JsonRequestBehavior.AllowGet); //1 == a true
        }

        public JsonResult estadoParejas(int id_encabezado)
        {
            var buscar = context.apareja_auditoriainv.Where(x => x.id_encabezado == id_encabezado).Select(x => new
            {
                numero = x.cod_pareja,
                persona1 = x.users.user_nombre + " " + x.users.user_apellido,
                persona2 = x.id_usuario_2 != null ? " - " + x.users.user_nombre + " " + x.users.user_apellido : "",
                estado = x.finalizado ? "Finalizado" : "En curso"
            }).ToList();

            var data = buscar.Select(x => new
            {
                x.numero,
                pareja = x.persona1 + x.persona2,
                x.estado
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult controlCambios(int id_encabezado)
        {
            List<int> conteos = context.aconteos_auditoria.Where(x => x.encabezado == id_encabezado).Select(x => x.id)
                .ToList();

            var modificaciones = context.amodificaciones_auditoriainv.Where(x => conteos.Contains(x.id_conteo)).Select(
                x => new
                {
                    referencia = "(" + x.aconteos_auditoria.icb_referencia.ref_codigo + ") " +
                                 x.aconteos_auditoria.icb_referencia.ref_descripcion,
                    conteo_1_anterior = x.conteo_1_anterior != null ? x.conteo_1_anterior.ToString() : "No aplica",
                    conteo_1_nuevo = x.conteo_1_nuevo != null ? x.conteo_1_nuevo.ToString() : "No aplica",
                    conteo_2_anterior = x.conteo_2_anterior != null ? x.conteo_2_anterior.ToString() : "No aplica",
                    conteo_2_nuevo = x.conteo_2_nuevo != null ? x.conteo_2_nuevo.ToString() : "No aplica",
                    usuario = x.users.user_nombre + " " + x.users.user_apellido,
                    fecha = x.fecha_modifica,
                    numeroPareja = x.aconteos_auditoria.apareja_auditoriainv.cod_pareja,
                    persona1 = x.aconteos_auditoria.apareja_auditoriainv.users.user_nombre + " " +
                               x.aconteos_auditoria.apareja_auditoriainv.users.user_apellido,
                    persona2 = x.aconteos_auditoria.apareja_auditoriainv.id_usuario_2 != null
                        ? " - " + x.aconteos_auditoria.apareja_auditoriainv.users1.user_nombre + " " +
                          x.aconteos_auditoria.apareja_auditoriainv.users1.user_apellido
                        : ""
                }).ToList();

            var data = modificaciones.Select(x => new
            {
                x.referencia,
                x.conteo_1_anterior,
                x.conteo_1_nuevo,
                x.conteo_2_nuevo,
                x.conteo_2_anterior,
                x.usuario,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm:ss"),
                pareja = x.numeroPareja + " - " + x.persona1 + x.persona2
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult asignarConteo3(int id_encab, string ubicaciones, string referencia, int pareja)
        {
            apareja_auditoriainv buscarPareja = context.apareja_auditoriainv.Find(pareja);
            buscarPareja.conteo_3 = true;
            buscarPareja.fin_conteo_3 = false;
            context.Entry(buscarPareja).State = EntityState.Modified;
            context.SaveChanges();

            string[] id_ubicaciones = ubicaciones.Split(',');
            int comparar = context.ubicaciones_asignadas_3
                .Where(x => x.id_encab == id_encab && x.ref_codigo == referencia).Count();
            if (comparar > 0)
            {
                return Json(0, JsonRequestBehavior.AllowGet); //0 = false 
            }

            ubicaciones_asignadas_3 extra = new ubicaciones_asignadas_3();
            foreach (string a in id_ubicaciones)
            {
                int ubi_id = Convert.ToInt32(a);
                extra.id_pareja = pareja;
                extra.id_ubicacion = ubi_id;
                extra.id_encab = id_encab;
                extra.ref_codigo = referencia;
                context.ubicaciones_asignadas_3.Add(extra);
                context.SaveChanges();
            }

            return Json(1, JsonRequestBehavior.AllowGet); // 1 = true
        }

        public JsonResult buscarConteoExtra()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);

            int buscarEncabezado = context.aencab_auditoriainv
                .FirstOrDefault(x => x.bodega == bodega && x.estado == true).id;
            int buscarPareja = context.apareja_auditoriainv
                .Where(x => x.id_encabezado == buscarEncabezado &&
                            (x.id_usuario_1 == usuario || x.id_usuario_2 == usuario) && x.conteo_3 &&
                            x.fin_conteo_3 == false).Select(x => x.id).FirstOrDefault();

            if (buscarPareja > 0)
            {
                return Json(new { extra = 1 }, JsonRequestBehavior.AllowGet); //1 = true
            }

            return Json(new { extra = 0 }, JsonRequestBehavior.AllowGet); // 0 = false
        }

        public JsonResult buscarAsignacionExtra(int encab, int pareja)
        {
            var buscarDatos = context.ubicaciones_asignadas_3.Where(x => x.id_encab == encab && x.id_pareja == pareja)
                .Select(x => new
                {
                    id = x.id_ubicacion,
                    x.ubicacion_repuestobod.descripcion
                }).Distinct().OrderBy(x => x.descripcion).ToList();

            return Json(buscarDatos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarReferenciasConteoExtra(int ubicacion, int encab, int pareja)
        {
            var buscar = context.ubicaciones_asignadas_3
                .Where(x => x.id_encab == encab && x.id_pareja == pareja && x.id_ubicacion == ubicacion).Select(x => new
                {
                    codigo = x.icb_referencia.ref_codigo,
                    descripcion = x.icb_referencia.ref_descripcion
                }).Distinct().ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarListaAsignada(int id_encab)
        {
            var buscar = context.ubicaciones_asignadas_3.Where(x => x.id_encab == id_encab).GroupBy(x => x.ref_codigo)
                .Select(grp => new
                {
                    ref_cod = grp.Select(x => x.ref_codigo).FirstOrDefault(),
                    ref_descripcion = grp.Select(x => x.icb_referencia.ref_descripcion).FirstOrDefault(),
                    ubicaciones = grp.Select(x => x.ubicacion_repuestobod.descripcion).ToList(),
                    codigo_grupo = grp.Select(x => x.apareja_auditoriainv.cod_pareja).FirstOrDefault(),
                    persona_1 = grp.Select(x => x.apareja_auditoriainv.users.user_nombre).FirstOrDefault() + " " +
                                grp.Select(x => x.apareja_auditoriainv.users.user_apellido).FirstOrDefault(),
                    persona_2 = grp.Select(x => x.apareja_auditoriainv.id_usuario_2).FirstOrDefault() != null
                        ? "-" + grp.Select(x => x.apareja_auditoriainv.users1.user_nombre).FirstOrDefault() + " " +
                          grp.Select(x => x.apareja_auditoriainv.users1.user_apellido).FirstOrDefault()
                        : ""
                }).ToList();

            var data = buscar.Select(x => new
            {
                referencia = x.ref_cod + " " + x.ref_descripcion,
                ubicaciones = string.Join(" , ", x.ubicaciones),
                grupo = x.codigo_grupo + " - " + x.persona_1 + x.persona_2
            }).OrderBy(x => x.grupo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult generarExcel(int id_encab)
        {
            var buscar = (from encab in context.aencab_auditoriainv
                          join pareja in context.apareja_auditoriainv
                              on encab.id equals pareja.id_encabezado
                          join conteo in context.aconteos_auditoria
                              on pareja.id equals conteo.pareja
                          join bod in context.bodega_concesionario
                              on encab.bodega equals bod.id
                          join refer in context.icb_referencia
                              on conteo.id_referencia equals refer.ref_codigo
                          join ubi in context.ubicacion_repuestobod
                              on conteo.ubicacion equals ubi.id
                          where encab.id == id_encab
                          select new
                          {
                              bodega = bod.bodccs_nombre,
                              inv = encab.descripcion,
                              grupo = pareja.cod_pareja,
                              referencia = refer.ref_codigo,
                              descripcion = refer.ref_descripcion,
                              ubicaciones = ubi.descripcion,
                              sistema = conteo.stock,
                              c1 = conteo.conteo1,
                              c2 = conteo.conteo2,
                              c_final = conteo.conteo3
                          }).ToList();

            var agrupar = buscar.GroupBy(x => x.referencia).Select(grp => new
            {
                bodega = grp.Select(x => x.bodega).FirstOrDefault(),
                inv = grp.Select(x => x.inv).FirstOrDefault(),
                grupo = string.Join(",", grp.Select(x => x.grupo).Distinct().ToList()),
                referencia = grp.Select(x => x.referencia).FirstOrDefault(),
                descripcion = grp.Select(x => x.descripcion).FirstOrDefault(),
                ubicaciones = string.Join(" , ", grp.Select(x => x.ubicaciones).Distinct().ToList()),
                sistema = grp.Select(x => x.sistema).FirstOrDefault(),
                c1 = grp.Sum(x => x.c1),
                c2 = grp.Sum(x => x.c2),
                c_final = grp.Sum(x => x.c_final),
                dif_1 = grp.Select(x => x.sistema).FirstOrDefault() - grp.Sum(x => x.c1),
                dif_2 = grp.Select(x => x.sistema).FirstOrDefault() - grp.Sum(x => x.c2),
                final = grp.Select(x => x.sistema).FirstOrDefault() - grp.Sum(x => x.c1) +
                        (grp.Select(x => x.sistema).FirstOrDefault() - grp.Sum(x => x.c2))
            }).ToList();

            if (agrupar.Count() > 0)
            {
                string nombre = agrupar[0].bodega + " - " + agrupar[0].inv + ".xlsx";

                string pathFile = AppDomain.CurrentDomain.BaseDirectory + "Informes Inventario\\" + nombre;
                SLDocument doc = new SLDocument();
                DataTable tabla = new DataTable();
                tabla.Columns.Add("Bodega", typeof(string));
                tabla.Columns.Add("Inv", typeof(string));
                tabla.Columns.Add("Grupo", typeof(string));
                tabla.Columns.Add("Referencia", typeof(string));
                tabla.Columns.Add("Descripcion", typeof(string));
                tabla.Columns.Add("Ubicaciones", typeof(string));
                tabla.Columns.Add("Sistema", typeof(string));
                tabla.Columns.Add("C1", typeof(string));
                tabla.Columns.Add("C2", typeof(string));
                tabla.Columns.Add("C FINAL", typeof(string));
                tabla.Columns.Add("DIF 1", typeof(string));
                tabla.Columns.Add("DIF 2", typeof(string));
                tabla.Columns.Add("FINAL", typeof(string));


                for (int i = 0; i < agrupar.Count(); i++)
                {
                    tabla.Rows.Add(
                        agrupar[i].bodega,
                        agrupar[i].inv,
                        agrupar[i].grupo,
                        agrupar[i].referencia,
                        agrupar[i].descripcion,
                        agrupar[i].ubicaciones,
                        agrupar[i].sistema,
                        agrupar[i].c1,
                        agrupar[i].c2,
                        agrupar[i].c_final,
                        agrupar[i].dif_1,
                        agrupar[i].dif_2,
                        agrupar[i].final
                    );
                }

                doc.ImportDataTable(1, 1, tabla, true);
                doc.SaveAs(pathFile);

                return Json(new { carga = 1, nombre }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { carga = 0, nombre = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}