using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class SzczegolyZamowienia
    {
        public int SzczegolyZamowieniaId { get; set; }
        public int ZamowienieId { get; set; }
        public int ProduktId { get; set; }
        public virtual Produkt Produkt { get; set; }
        public virtual Zamowienie Zamowienie { get; set; }
        public int Sztuk { get; set; }
        public decimal UnitPrice { get; set; }

    }
}