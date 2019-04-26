using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Producent
    {
        [Key]
        public int ProducentId { get; set; }
        public string Nazwa { get; set; }

    }
}