using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Adres
    {
        [Key]
        public int AdresId { get; set; }
        public string Miasto { get; set; }
        public string Ulica { get; set; }
        public int Numer { get; set; }
        public string KodPocztowy { get; set; }

    }
}