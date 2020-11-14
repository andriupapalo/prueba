using Homer_MVC.IcebergModel;
using System;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class rremplazosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: rremplazos
        public ActionResult Index(int? menu)
        {
            ViewBag.datos = db.rremplazos.ToList();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: rprecios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult carguetxt(HttpPostedFileBase txtCargue, int? menu)
        {
            try
            {
                string path = Server.MapPath("~/Content/" + txtCargue.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtCargue.SaveAs(path);

                StreamReader objReader = new StreamReader(path);
                string linea = "";
                ArrayList arrText = new ArrayList();
                bool encabezado = true;

                while (linea != null)
                {
                    linea = objReader.ReadLine();
                    if (encabezado == false)
                    {
                        if (linea != null)
                        {
                            arrText.Add(linea);
                        }
                    }

                    encabezado = false;
                }

                objReader.Close();

                foreach (string item in arrText)
                {
                    string referencia = item.Substring(10, 18).Trim();
                    string alterno = item.Substring(78, item.Length - 78).Trim();
                    string nombre = item.Substring(28, 40).Trim();

                    //quitarle los 0 a la referencia
                    string ceros = item.Substring(10, 5);
                    int i = 0;
                    if (ceros == "00000")
                    {
                        while (item.Substring(10 + i, 1) == "0")
                        {
                            i++;
                            referencia = item.Substring(10 + i, 18 - i);
                        }
                    }

                    //quitarle los 0 al alterno
                    string cerosal = item.Substring(78, 4);
                    int j = 0;
                    if (cerosal == "0000")
                    {
                        while (item.Substring(78 + j, 1) == "0")
                        {
                            j++;
                            alterno = item.Substring(78 + j, item.Length - 78 - j);
                        }
                    }

                    rremplazos existe = db.rremplazos.FirstOrDefault(x => x.referencia == referencia);

                    rremplazos reemplazo = new rremplazos();
                    if (existe == null)
                    {
                        reemplazo.descripcion = nombre;
                        reemplazo.referencia = referencia;
                        reemplazo.alterno = alterno;
                        db.rremplazos.Add(reemplazo);

                        db.SaveChanges();
                        System.Collections.Generic.List<rremplazos> r = db.rremplazos.ToList();

                        foreach (rremplazos itema in r)
                        {
                            if (itema.idpadre == null)
                            {
                                var origen = (from rr in db.rremplazos
                                              select new
                                              {
                                                  rr.referencia,
                                                  rr.id,
                                                  rr.idpadre,
                                                  alterno = (from al in db.rremplazos
                                                             where al.alterno == rr.referencia
                                                             select new { al.alterno, al.id, al.idpadre }).FirstOrDefault()
                                              }).ToList();

                                foreach (var it in origen)
                                {
                                    if (it.idpadre == null)
                                    {
                                        rremplazos rem = db.rremplazos.Find(it.id);
                                        if (it.alterno == null)
                                        {
                                            rem.idpadre = Convert.ToInt32(it.id);
                                        }
                                        else
                                        {
                                            rem.idpadre = it.alterno.idpadre;
                                        }

                                        db.Entry(rem).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                TempData["mensaje"] = "El archivo se ha cargado correctamente";
                return RedirectToAction("Index", "rremplazos", new { menu });
            }
            catch (DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity,
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }

                TempData["mensaje_error"] = raise;
                return RedirectToAction("Index", "rremplazos", new { menu });
            }
        }

        /*try
		{
		    db.SaveChanges();
		    var r = db.rremplazos.ToList();
		    //var existe = db.rremplazos.FirstOrDefault(x => x.referencia == Convert.ToString(r.Select(a=> a.referencia)));

		    foreach (var item in r)
		    {
		        //var origen = db.rremplazos.Where(t => t.alterno.Contains(t.referencia)).FirstOrDefault();
		        var origen = (from rr in db.rremplazos
		                        select new
		                        {
		                            rr.referencia,
		                            rr.id,
		                            rr.idpadre,
		                            alterno = (from al in db.rremplazos
		                                        where al.alterno == rr.referencia
		                                        select new { al.alterno, al.id, al.idpadre }).FirstOrDefault()
		                        }).ToList();

		        foreach (var i in origen)
		        {
		            var rem = db.rremplazos.Find(i.id);
		            if (i.alterno == null)
		            {
		                rem.idpadre = Convert.ToInt32(i.id);
		            }
		            else
		            {
		                rem.idpadre = i.alterno.idpadre;
		            }
		            db.Entry(rem).State = EntityState.Modified;
		            db.SaveChanges();                                
		        }

		    }

		}
		catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
		{
		    Exception raise = dbEx;
		    foreach (var validationErrors in dbEx.EntityValidationErrors)
		    {
		        foreach (var validationError in validationErrors.ValidationErrors)
		        {
		            string message = string.Format("{0}:{1}",
		                validationErrors.Entry.Entity.ToString(),
		                validationError.ErrorMessage);
		            // raise a new exception nesting
		            // the current instance as InnerException
		            raise = new InvalidOperationException(message, raise);
		        }
		    }
		    throw raise;
		}


		TempData["mensaje"] = "El archivo se ha cargado correctamente";
		return RedirectToAction("Index", "rremplazos", new { menu });
	}
	catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
	{
		Exception raise = dbEx;
		foreach (var validationErrors in dbEx.EntityValidationErrors)
		{
		    foreach (var validationError in validationErrors.ValidationErrors)
		    {
		        string message = string.Format("{0}:{1}",
		            validationErrors.Entry.Entity.ToString(),
		            validationError.ErrorMessage);
		        // raise a new exception nesting
		        // the current instance as InnerException
		        raise = new InvalidOperationException(message, raise);
		    }
		}
		TempData["mensaje_error"] = raise;
		return RedirectToAction("Index", "rremplazos", new { menu });
	}
}*/

        public JsonResult BuscarDatos()
        {
            var data = from e in db.rremplazos
                       select new
                       {
                           e.referencia,
                           e.alterno
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
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