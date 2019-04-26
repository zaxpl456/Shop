using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class ZamowioneSztuki
    {
        [Key]
        public int KluczId { get; set; }

        public int ProduktId { get; set; }
        public int LiczbaSztuk { get; set; }
    }
}