using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using IdentitySample.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity.Owin;
using Shop.Models;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Shop.ViewModel;

namespace Shop.Controllers
{
    [Authorize]
    public class PodsumowanieController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        ApplicationDbContext db = new ApplicationDbContext();


        // GET: Podsumowanie
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);



            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()

            };

            ViewBag.Dostawa = db.Dostawas.AsNoTracking().ToList();

            ViewBag.Adres = db.Profil.Single(p => p.NazwaUzytkownika == User.Identity.Name).Adresy.ToList();


            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Index(FormCollection values)
        {

            Zamowienie zamowienie = new Zamowienie();
            if (Request.Form["Adres"] ==null)
            {
                return RedirectToAction("Error", new { id = 1 });

            }
            else if(Request.Form["Dostawa"] == null)
            {
                return RedirectToAction("Error", new { id = 2 });

            }
            int s = Int16.Parse(Request.Form["Adres"]);

            int s1 = Int16.Parse(Request.Form["Dostawa"]);
           

            try
            {


                zamowienie.OrderDate = DateTime.Now;
                zamowienie.AdresId = s;
                zamowienie.DostawaId = s1;
                zamowienie.Stan = 0;
                zamowienie.Produkty=new List<Produkt>();
                    Profil profil = db.Profil.AsNoTracking().Single(p => p.NazwaUzytkownika == User.Identity.Name);

                zamowienie.ProfilId = profil.ProfilId;
                zamowienie.Sztuki=new List<ZamowioneSztuki>();
                List<Koszyk> koszyks = db.Koszyks.Where(p => p.KoszykId == profil.NazwaUzytkownika).ToList();
                foreach (var koszyk in koszyks)
                {
                    ZamowioneSztuki zamowione = new ZamowioneSztuki
                    {
                       
                        ProduktId = koszyk.Produkt.ProduktId,
                        LiczbaSztuk = koszyk.Count
                        
                       
                    };
                    db.ZamowioneSztukis.Add(zamowione);
                    
                    zamowienie.Sztuki.Add(zamowione);
                    koszyk.Produkt.Sztuk = koszyk.Produkt.Sztuk - koszyk.Count;

                    zamowienie.Produkty.Add(koszyk.Produkt);
                    zamowienie.Koszt+=(koszyk.Count * koszyk.Produkt.Cena);
                    
                    db.Koszyks.Remove(koszyk);
                }
                
                zamowienie.Koszt += db.Dostawas.Single(d => d.DostawaId == s1).Cena;

                db.Zamowienies.Add(zamowienie);

                db.SaveChanges();

                GeneratePdf(zamowienie);

                SendMail(zamowienie);



                return RedirectToAction("Sukcess", new {id = zamowienie.ZamowienieId});
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception);
                return View();
            }

        }
        public ActionResult Error(int id)
        {
            return RedirectToAction("Index", new { kom = "1" });

            return View();
        }
        public ActionResult Sukcess(int id)
        {
     
            return View();
        }


     

        #region--Generate Invoice PDF--
        public void GeneratePdf(Zamowienie zamowienie)
        {
            Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
            var output = new FileStream(Server.MapPath("Faktura-"+zamowienie.ZamowienieId+".pdf"), FileMode.Create);
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            pdfDoc.Open();
            var titleFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var titleFontBlue = FontFactory.GetFont("Arial", 14, Font.NORMAL, BaseColor.BLUE);
            var boldTableFont = FontFactory.GetFont("Arial", 8, Font.BOLD);
            var bodyFont = FontFactory.GetFont("Arial", 8, Font.NORMAL);
            var EmailFont = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLUE);
            BaseColor TabelHeaderBackGroundColor = WebColors.GetRGBColor("#EEEEEE");

            Rectangle pageSize = writer.PageSize;
            // Open the Document for writing
            pdfDoc.Open();
            //Add elements to the document here

            #region Top table
            // Create the header table 
            PdfPTable headertable = new PdfPTable(3);
            headertable.HorizontalAlignment = 0;
            headertable.WidthPercentage = 100;
            headertable.SetWidths(new float[] { 100f, 320f, 100f });  // then set the column's __relative__ widths
            headertable.DefaultCell.Border = Rectangle.NO_BORDER;
            //headertable.DefaultCell.Border = Rectangle.BOX; //for testing           

            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(HttpContext.ApplicationInstance.Server.MapPath("~/images/logo.png"));
            logo.ScaleToFit(100, 15);

            {
                PdfPCell pdfCelllogo = new PdfPCell(logo);
                pdfCelllogo.Border = Rectangle.NO_BORDER;
                pdfCelllogo.BorderColorBottom = new BaseColor(System.Drawing.Color.Black);
                pdfCelllogo.BorderWidthBottom = 1f;
                headertable.AddCell(pdfCelllogo);
            }

            {
                PdfPCell middlecell = new PdfPCell();
                middlecell.Border = Rectangle.NO_BORDER;
                middlecell.BorderColorBottom = new BaseColor(System.Drawing.Color.Black);
                middlecell.BorderWidthBottom = 1f;
                headertable.AddCell(middlecell);
            }

            {
                PdfPTable nested = new PdfPTable(1);
                nested.DefaultCell.Border = Rectangle.NO_BORDER;
                PdfPCell nextPostCell1 = new PdfPCell(new Phrase("BEST SZOP", titleFont));
                nextPostCell1.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell1);
                PdfPCell nextPostCell2 = new PdfPCell(new Phrase("Best SZOP, Siedlce ul.Maja3,", bodyFont));
                nextPostCell2.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell2);
                PdfPCell nextPostCell3 = new PdfPCell(new Phrase("08-110", bodyFont));
                nextPostCell3.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell3);
                PdfPCell nextPostCell4 = new PdfPCell(new Phrase("BestSzop@uph.com", EmailFont));
                nextPostCell4.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell4);
                nested.AddCell("");
                PdfPCell nesthousing = new PdfPCell(nested);
                nesthousing.Border = Rectangle.NO_BORDER;
                nesthousing.BorderColorBottom = new BaseColor(System.Drawing.Color.Black);
                nesthousing.BorderWidthBottom = 1f;
                nesthousing.Rowspan = 5;
                nesthousing.PaddingBottom = 10f;
                headertable.AddCell(nesthousing);
            }


            PdfPTable Invoicetable = new PdfPTable(3);
            Invoicetable.HorizontalAlignment = 0;
            Invoicetable.WidthPercentage = 100;
            Invoicetable.SetWidths(new float[] { 100f, 320f, 100f });  // then set the column's __relative__ widths
            Invoicetable.DefaultCell.Border = Rectangle.NO_BORDER;
            Adres adres=db.Adres.Single(a => a.AdresId == zamowienie.AdresId);
            {
                PdfPTable nested = new PdfPTable(1);
                nested.DefaultCell.Border = Rectangle.NO_BORDER;
                PdfPCell nextPostCell1 = new PdfPCell(new Phrase("Zamówienie dla:", bodyFont));
                nextPostCell1.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell1);
                PdfPCell nextPostCell2 = new PdfPCell(new Phrase(User.Identity.Name.TrimEnd('@'), titleFont));
                nextPostCell2.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell2);
                PdfPCell nextPostCell3 = new PdfPCell(new Phrase(adres.Miasto+", "+adres.KodPocztowy+", "+adres.Ulica+", "+adres.Numer, bodyFont));
                nextPostCell3.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell3);
                PdfPCell nextPostCell4 = new PdfPCell(new Phrase(User.Identity.Name, EmailFont));
                nextPostCell4.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell4);
                nested.AddCell("");
                
                PdfPCell nesthousing = new PdfPCell(nested);
                nesthousing.Border = Rectangle.NO_BORDER;
             
                nesthousing.Rowspan = 5;
                nesthousing.PaddingBottom = 10f;
                Invoicetable.AddCell(nesthousing);
            }

            {
                PdfPCell middlecell = new PdfPCell();
                middlecell.Border = Rectangle.NO_BORDER;
                //middlecell.BorderColorBottom = new BaseColor(System.Drawing.Color.Black);
                //middlecell.BorderWidthBottom = 1f;
                Invoicetable.AddCell(middlecell);
            }


            {
                PdfPTable nested = new PdfPTable(1);
                nested.DefaultCell.Border = Rectangle.NO_BORDER;
                PdfPCell nextPostCell1 = new PdfPCell(new Phrase("Zamówienie", titleFontBlue));
                nextPostCell1.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell1);
                PdfPCell nextPostCell2 = new PdfPCell(new Phrase("Data zamówienia: " + zamowienie.OrderDate, bodyFont));
                nextPostCell2.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell2);
                PdfPCell nextPostCell3 = new PdfPCell(new Phrase("Realizacja: " + DateTime.Now.AddDays(2).ToShortDateString(), bodyFont));
                nextPostCell3.Border = Rectangle.NO_BORDER;
                nested.AddCell(nextPostCell3);
                nested.AddCell("");
                PdfPCell nesthousing = new PdfPCell(nested);
                nesthousing.Border = Rectangle.NO_BORDER;

                nesthousing.Rowspan = 5;
                nesthousing.PaddingBottom = 10f;
                Invoicetable.AddCell(nesthousing);
            }


            pdfDoc.Add(headertable);
            Invoicetable.PaddingTop = 10f;

            pdfDoc.Add(Invoicetable);
            #endregion

            #region Items Table
     
            PdfPTable itemTable = new PdfPTable(5);

            itemTable.HorizontalAlignment = 0;
            itemTable.WidthPercentage = 100;
            itemTable.SetWidths(new float[] { 5, 40, 10, 20, 25 });  // then set the column's __relative__ widths
            itemTable.SpacingAfter = 40;
            itemTable.DefaultCell.Border = Rectangle.BOX;
            PdfPCell cell1 = new PdfPCell(new Phrase("Id", boldTableFont));
            cell1.BackgroundColor = TabelHeaderBackGroundColor;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
            itemTable.AddCell(cell1);
            PdfPCell cell2 = new PdfPCell(new Phrase("Nazwa", boldTableFont));
            cell2.BackgroundColor = TabelHeaderBackGroundColor;
            cell2.HorizontalAlignment = 1;
            itemTable.AddCell(cell2);


            PdfPCell cell3 = new PdfPCell(new Phrase("Sztuk", boldTableFont));
            cell3.BackgroundColor = TabelHeaderBackGroundColor;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            itemTable.AddCell(cell3);
            PdfPCell cell4 = new PdfPCell(new Phrase("Cena jednostkowa", boldTableFont));
            cell4.BackgroundColor = TabelHeaderBackGroundColor;
            cell4.HorizontalAlignment = Element.ALIGN_CENTER;
            itemTable.AddCell(cell4);
            PdfPCell cell5 = new PdfPCell(new Phrase("Całkowity koszt", boldTableFont));
            cell5.BackgroundColor = TabelHeaderBackGroundColor;
            cell5.HorizontalAlignment = Element.ALIGN_CENTER;
            itemTable.AddCell(cell5);
            foreach (var produkt in zamowienie.Produkty)
            {
                PdfPCell numberCell = new PdfPCell(new Phrase(produkt.ProduktId.ToString(), bodyFont));
                numberCell.HorizontalAlignment = 1;
                numberCell.PaddingLeft = 10f;
                numberCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                itemTable.AddCell(numberCell);

                var _phrase = new Phrase();
                _phrase.Add(new Chunk(produkt.Nazwa, bodyFont));
                PdfPCell descCell = new PdfPCell(_phrase);
                descCell.HorizontalAlignment = 0;
                descCell.PaddingLeft = 10f;
                descCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                itemTable.AddCell(descCell);
             
                ZamowioneSztuki z= zamowienie.Sztuki.Single(p => p.ProduktId==produkt.ProduktId);
                PdfPCell qtyCell = new PdfPCell(new Phrase(z.LiczbaSztuk.ToString(), bodyFont));
                qtyCell.HorizontalAlignment = 1;
                qtyCell.PaddingLeft = 10f;
                qtyCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                itemTable.AddCell(qtyCell);

                PdfPCell amountCell = new PdfPCell(new Phrase(produkt.Cena.ToString(), bodyFont));
                amountCell.HorizontalAlignment = 1;
                amountCell.PaddingLeft = 10f;
                amountCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                itemTable.AddCell(amountCell);

                PdfPCell totalamtCell = new PdfPCell(new Phrase((produkt.Cena*z.LiczbaSztuk).ToString(), bodyFont));
                totalamtCell.HorizontalAlignment = 1;
                totalamtCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                itemTable.AddCell(totalamtCell);

            }
            // Table footer
            PdfPCell totalAmtCell1 = new PdfPCell(new Phrase(""));
            totalAmtCell1.Border = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER;
            itemTable.AddCell(totalAmtCell1);
            PdfPCell totalAmtCell2 = new PdfPCell(new Phrase(""));
            totalAmtCell2.Border = Rectangle.TOP_BORDER; //Rectangle.NO_BORDER; //Rectangle.TOP_BORDER;
            itemTable.AddCell(totalAmtCell2);
            PdfPCell totalAmtCell3 = new PdfPCell(new Phrase(""));
            totalAmtCell3.Border = Rectangle.TOP_BORDER; //Rectangle.NO_BORDER; //Rectangle.TOP_BORDER;
            itemTable.AddCell(totalAmtCell3);
            PdfPCell totalAmtStrCell = new PdfPCell(new Phrase("Koszt całkowity", boldTableFont));
            totalAmtStrCell.Border = Rectangle.TOP_BORDER;   //Rectangle.NO_BORDER; //Rectangle.TOP_BORDER;
            totalAmtStrCell.HorizontalAlignment = 1;
            itemTable.AddCell(totalAmtStrCell);
            PdfPCell totalAmtCell = new PdfPCell(new Phrase(zamowienie.Koszt.ToString(), boldTableFont));
            totalAmtCell.HorizontalAlignment = 1;
            itemTable.AddCell(totalAmtCell);
            Dostawa dostawa = db.Dostawas.Single(d => d.DostawaId == zamowienie.DostawaId);
            PdfPCell cell = new PdfPCell(new Phrase("", bodyFont));
            cell.Colspan = 5;
            cell.HorizontalAlignment = 1;
            itemTable.AddCell(cell);

            pdfDoc.Add(itemTable);
            #endregion

            PdfContentByte cb = new PdfContentByte(writer);


            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, true);
            cb = new PdfContentByte(writer);
            cb = writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.SetTextMatrix(pageSize.GetLeft(120), 20);
            cb.ShowText("Dokument został wygenerowany automatycznie ");
            cb.EndText();

            //Move the pointer and draw line to separate footer section from rest of page
            cb.MoveTo(40, pdfDoc.PageSize.GetBottom(50));
            cb.LineTo(pdfDoc.PageSize.Width - 40, pdfDoc.PageSize.GetBottom(50));
            cb.Stroke();

            pdfDoc.Close();
         


        }
        #endregion


        public void SendMail(Zamowienie zamowienie)
        {
            MailMessage message = new MailMessage(ConfigurationManager.AppSettings["sender"], User.Identity.Name)
            {
                Subject = "Potwierdzenie zamówienia",
                Body = "Witaj twoje zamówienie zostało potwierdzone, faktura znajduję się w załączniku",

            };

            message.Attachments.Add(new Attachment(Server.MapPath("/Faktura-" + zamowienie.ZamowienieId + ".pdf")));

            SmtpClient smtpClient = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["smtpHost"],
                Credentials = new NetworkCredential(
                    ConfigurationManager.AppSettings["sender"],
                    ConfigurationManager.AppSettings["passwd"]),
                EnableSsl = true,
                Port = 587,

            };
            smtpClient.Send(message);
        }
       




    }
}