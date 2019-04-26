using System.Collections.Generic;

using Shop.Models;

namespace Shop.ViewModel
{
    public class ShoppingCartViewModel 
    {
        public List<Koszyk> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}