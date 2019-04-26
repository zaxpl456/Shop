using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Koszyk
    {
        [Key]
        public int ItemId { get; set; }

        public string KoszykId { get; set; }

        public int Count { get; set; }

        public System.DateTime DateCreated { get; set; }

        public int ProduktId { get; set; }

        public virtual Produkt Produkt { get; set; }
    }
}