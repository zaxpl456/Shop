using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Shop.Models
{
    public class Kategoria
    {
        public int Id { get; set; }

        public string Nazwa { get; set; }

        public int? ParentId { get; set; }
        public virtual Kategoria Parent { get; set; }


        public virtual ICollection<Kategoria> Children { get; set; }

        public virtual ICollection<Produkt> Produkts { get; set; }


        public class Mapping : EntityTypeConfiguration<Kategoria>
        {
            public Mapping()
            {
                HasOptional(m => m.Parent).WithMany(m => m.Children);
            }
        }
    }
}