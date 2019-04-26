using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Shop.Models
{
    public class Produkt
    {
        [Key]
        public int ProduktId { get; set; }
        [StringLength(50,ErrorMessage="Nazwa nie może mieć więcej niż 50 znaków")]
        [DisplayName("Nazwa produktu")]
        [Required]
        public string Nazwa { get; set; }
        [DataType(DataType.Currency)]
        [Range(0.01,100000.00,ErrorMessage = "Błędna cena")]
        [Required]

        public decimal Cena { get; set; }
        [DisplayName("Liczba sztuk")]
        [Required]

        public int Sztuk { get; set; }
        public int ProducentId { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd")]
        [Required]
        [DisplayName("Data produkcji")]
        public DateTime RokProdukcji { get; set; }
        public virtual ICollection<Kategoria> Kategoria { get; set; }


        [DataType((DataType.ImageUrl))]
        public string Obrazek { get; set; }
   

        public virtual Producent Producent { get; set; }


    }
}