using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using IdentitySample.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using Shop.Models;
using Shop.ViewModel;

namespace Shop.Controllers
{
    public class ProduktsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Produkts
        public ActionResult Index(string sortOrder,string searchString,string currentFilter,int? page,int? kat)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParam = sortOrder == "Price" ? "price_desc" : "Price";

            if (searchString!=null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var produkts_s = from p in db.Produkts select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                produkts_s = produkts_s.Where(s => s.Nazwa.Contains(searchString));
            }

            if (kat != null)
            {
                produkts_s = produkts_s
                    .Where(s => s.Kategoria
                        .Any(p => p.Id == kat));

            }

            switch (sortOrder)
            {
                case "name_desc":
                   produkts_s= produkts_s.OrderBy(p => p.Nazwa);
                    break;
                case "name_asc":
                    produkts_s = produkts_s.OrderBy(p => p.Nazwa);
                    break;
                case "Price":
                    produkts_s = produkts_s.OrderBy(p => p.Cena);
                    break;
                case "price_desc":
                    produkts_s = produkts_s.OrderByDescending(p => p.Cena);
                    break;
                default:
                    produkts_s = produkts_s.OrderByDescending(p => p.Nazwa);
                    break;


            }
            
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            
           

            return View(produkts_s.ToPagedList(pageNumber,pageSize));
        }

        // GET: Produkts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkt produkt = db.Produkts.Find(id);
            if (produkt == null)
            {
                return HttpNotFound();
            }
            return View(produkt);
        }

        // GET: Produkts/Create
        [Authorize(Roles = "Admin,Moderator")]

        public ActionResult Create()
        {
            
            ViewBag.ProducentId = new SelectList(db.Producents, "ProducentId", "Nazwa");
            var produkt = new Produkt();
            produkt.Kategoria=new List<Kategoria>();
            Przypisanie(produkt);
            return View();
        }

        // POST: Produkts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]

        public ActionResult Create([Bind(Include = "ProduktId,Nazwa,Sztuk,ProducentId,RokProdukcji")] Produkt produkt,string[] selectedKat,string cena)
        {
            Decimal d=Decimal.Parse(cena);
            produkt.Cena = d;
            if (selectedKat != null)
            {
                produkt.Kategoria = new List<Kategoria>();
                foreach (var kat in selectedKat)
                {
                    var katToAdd = db.Kategorias.Find(int.Parse(kat));
                    produkt.Kategoria.Add(katToAdd);
                }
            }

            HttpPostedFileBase file = Request.Files["plik"];
            if (file != null && file.ContentLength > 0)
            {
                produkt.Obrazek = file.FileName;
                file.SaveAs(HttpContext.Server.MapPath("~/Obrazki/")+produkt.Obrazek);
            }

            
                db.Produkts.Add(produkt);
                db.SaveChanges();
                return RedirectToAction("Index");
            

            ViewBag.ProducentId = new SelectList(db.Producents, "ProducentId", "Nazwa", produkt.ProducentId);
            return View(produkt);
        }
        [Authorize(Roles = "Admin,Moderator")]

        // GET: Produkts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           // Produkt produkt = db.Produkts.Find(id);
            Produkt produkt = db.Produkts
                .Include(k => k.Kategoria)
                .Single(i => i.ProduktId == id);
            if (produkt == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProducentId = new SelectList(db.Producents, "ProducentId", "Nazwa", produkt.ProducentId);
            Przypisanie(produkt);

            return View(produkt);
        }

        // POST: Produkts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]

        public ActionResult Edit(int? id, string[] selectedKat)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var produktToUpdate = db.Produkts
                .Include(k => k.Kategoria)
                .Single(i => i.ProduktId == id);
            if (TryUpdateModel(produktToUpdate, "",
                new string[] {"ProduktId", "Nazwa", "Cena", "Sztuk", "ProducentId", "RokProdukcji"}))
            {
                try
                {
                   UpdateKategory(selectedKat,produktToUpdate);
                }
                catch (RetryLimitExceededException dex)
                {
                    Console.WriteLine(dex);
                    throw;
                }
            }
            
                db.Entry(produktToUpdate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            
            ViewBag.ProducentId = new SelectList(db.Producents, "ProducentId", "Nazwa", produktToUpdate.ProducentId);
            return View(produktToUpdate);
        }

        // GET: Produkts/Delete/5
        [Authorize(Roles = "Admin,Moderator")]

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkt produkt = db.Produkts.Find(id);
            if (produkt == null)
            {
                return HttpNotFound();
            }
            return View(produkt);
        }

        // POST: Produkts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]

        public ActionResult DeleteConfirmed(int id)
        {
            Produkt produkt = db.Produkts.Find(id);
            db.Produkts.Remove(produkt);
           
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        private void Przypisanie(Produkt produkt)
        {
            var kategorie = db.Kategorias;
            var dopisane = new HashSet<int>(produkt.Kategoria.Select(k => k.Id));
            var viewModel = new List<KategoriaVM>();
            foreach (var kat in kategorie)
            {

                    viewModel.Add(new KategoriaVM
                    {

                        KategoriaId = kat.Id,
                        KatNazwa = kat.Nazwa,
                        Check = dopisane.Contains(kat.Id)
                    });
               
            }
            ViewBag.Kat = viewModel;
        }

        private void UpdateKategory(string[] selectedKat, Produkt produktToUpdate)
        {
            if (selectedKat == null)
            {
                produktToUpdate.Kategoria=new List<Kategoria>();
                return;
            }

            var selectedKatHS=new HashSet<string>(selectedKat);
            var produktKat=new HashSet<int>(produktToUpdate.Kategoria.Select(k=>k.Id));
            foreach (var kat in db.Kategorias)
            {
                if (selectedKatHS.Contains(kat.Id.ToString()))
                {
                    if (!produktKat.Contains(kat.Id))
                    {
                        produktToUpdate.Kategoria.Add(kat);
                    }
                }
                else
                {
                    if (produktKat.Contains(kat.Id))
                    {
                        produktToUpdate.Kategoria.Remove(kat);
                    }
                }

            }
        }

        [ChildActionOnly]
        public ActionResult KategoriaMenu()
        {
            var kategorias = db.Kategorias.ToList();
            return PartialView(kategorias);
        }
    }
}
