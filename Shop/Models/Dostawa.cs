using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Dostawa
    {
        [Key]
        public int DostawaId { get; set; }
        public string Nazwa { get; set; }
        public decimal Cena { get; set; }

        

    }
}