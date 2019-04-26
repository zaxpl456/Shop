using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Zamowienie
    {
        [Key]
        public int ZamowienieId { get; set; }
        public int Stan { get; set; }

        public int DostawaId { get; set; }

        public Dostawa Dostawa { get; set; }

        public int AdresId { get; set; }
        public Adres Adres { get; set; }
        public Profil Profil { get; set; }
        public int ProfilId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public decimal Koszt { get; set; }

        public ICollection<ZamowioneSztuki> Sztuki { get; set; }
        public virtual ICollection<Produkt> Produkty { get; set; }

    }
}