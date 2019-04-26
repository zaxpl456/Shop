using System;
using System.Linq;
using System.Web.Mvc;
using IdentitySample.Models;
using Shop.Models;
using Shop.ViewModel;

namespace IdentitySample.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db=new ApplicationDbContext();
        public ActionResult Index()
        {
           
            return View(db.Produkts.ToList());
        }

        [Authorize]
        public ActionResult About()
        {
            

            IQueryable<EnrollmentDateGroup> data = from Produkt in db.Produkts
                                                   
                                                   group Produkt by Produkt.RokProdukcji  into dateGroup
                                                   select new EnrollmentDateGroup()
                                                   {
                                                       EnrollmentDate =dateGroup.Key,
                                                       ProductCount = dateGroup.Count()
                                                   };
            return View(data.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
