using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class Profil
    {
        [Key]
        public int ProfilId { get; set; }
        public string NazwaUzytkownika { get; set; }
        public virtual ICollection<Adres> Adresy { get; set; }

    }
}