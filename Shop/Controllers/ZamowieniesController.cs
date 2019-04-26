using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IdentitySample.Models;
using Shop.Models;

namespace Shop.Controllers
{
    public class ZamowieniesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Zamowienies
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var zamowienies = db.Zamowienies.Include(z => z.Adres).Include(z => z.Dostawa).Include(z => z.Profil);
            return View(zamowienies.ToList());
        }

        public ActionResult UserZamowienies()
        {
            Profil profil = db.Profil.Single(p => p.NazwaUzytkownika == User.Identity.Name);
            var zamowienies = db.Zamowienies.Include(z => z.Adres).Include(z => z.Dostawa).Include(z => z.Profil).Where(s=>s.ProfilId==profil.ProfilId);

            return View(zamowienies.ToList());
        }

        // GET: Zamowienies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var zamowienie = db.Zamowienies.Include(z => z.Adres).Include(z => z.Dostawa).Include(z => z.Profil).Include(z=>z.Sztuki).Single(z=>z.ZamowienieId==id);

            if (zamowienie == null)
            {
                return HttpNotFound();
            }
            return View(zamowienie);
        }

        // GET: Zamowienies/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.AdresId = new SelectList(db.Adres, "AdresId", "Miasto");
            ViewBag.DostawaId = new SelectList(db.Dostawas, "DostawaId", "Nazwa");
            ViewBag.ProfilId = new SelectList(db.Profil, "ProfilId", "NazwaUzytkownika");
            return View();
        }

        // POST: Zamowienies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ZamowienieId,Stan,DostawaId,AdresId,ProfilId,OrderDate,Koszt")] Zamowienie zamowienie)
        {
            if (ModelState.IsValid)
            {
                db.Zamowienies.Add(zamowienie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdresId = new SelectList(db.Adres, "AdresId", "Miasto", zamowienie.AdresId);
            ViewBag.DostawaId = new SelectList(db.Dostawas, "DostawaId", "Nazwa", zamowienie.DostawaId);
            ViewBag.ProfilId = new SelectList(db.Profil, "ProfilId", "NazwaUzytkownika", zamowienie.ProfilId);
            return View(zamowienie);
        }

        // GET: Zamowienies/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Zamowienie zamowienie = db.Zamowienies.Find(id);
            if (zamowienie == null)
            {
                return HttpNotFound();
            }
            ViewBag.AdresId = new SelectList(db.Adres, "AdresId", "Miasto", zamowienie.AdresId);
            ViewBag.DostawaId = new SelectList(db.Dostawas, "DostawaId", "Nazwa", zamowienie.DostawaId);
            ViewBag.ProfilId = new SelectList(db.Profil, "ProfilId", "NazwaUzytkownika", zamowienie.ProfilId);
            return View(zamowienie);
        }

        // POST: Zamowienies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ZamowienieId,Stan,DostawaId,AdresId,ProfilId,OrderDate,Koszt")] Zamowienie zamowienie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(zamowienie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AdresId = new SelectList(db.Adres, "AdresId", "Miasto", zamowienie.AdresId);
            ViewBag.DostawaId = new SelectList(db.Dostawas, "DostawaId", "Nazwa", zamowienie.DostawaId);
            ViewBag.ProfilId = new SelectList(db.Profil, "ProfilId", "NazwaUzytkownika", zamowienie.ProfilId);
            return View(zamowienie);
        }

        // GET: Zamowienies/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Zamowienie zamowienie = db.Zamowienies.Find(id);
            if (zamowienie == null)
            {
                return HttpNotFound();
            }
            return View(zamowienie);
        }

        // POST: Zamowienies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Zamowienie zamowienie = db.Zamowienies.Find(id);
            db.Zamowienies.Remove(zamowienie);
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
    }
}
