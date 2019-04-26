using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IdentitySample.Models;

namespace Shop.Models
{
    public partial class ShoppingCart
    {
        ApplicationDbContext storeDB = new ApplicationDbContext();
        string ShoppingCartId { get; set; }
        public const string CartSessionKey = "CartId";
        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }
        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }
        public void AddToCart(Produkt produkt,int sztuk)
        {
            // Get the matching cart and album instances
            var cartItem = storeDB.Koszyks.SingleOrDefault(
                c => c.KoszykId == ShoppingCartId
                && c.ProduktId == produkt.ProduktId);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Koszyk
                {
                    ProduktId = produkt.ProduktId,
                    KoszykId = ShoppingCartId,
                    Count = sztuk,
                    DateCreated = DateTime.Now
                };
                storeDB.Koszyks.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, 
                // then add one to the quantity
                cartItem.Count++;
            }
            // Save changes
            storeDB.SaveChanges();
        }
        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = storeDB.Koszyks.Single(
                cart => cart.KoszykId == ShoppingCartId
                && cart.ItemId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    storeDB.Koszyks.Remove(cartItem);
                }
                // Save changes
                storeDB.SaveChanges();
            }
            return itemCount;
        }
        public void EmptyCart()
        {
            var cartItems = storeDB.Koszyks.Where(
                cart => cart.KoszykId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                storeDB.Koszyks.Remove(cartItem);
            }
            // Save changes
            storeDB.SaveChanges();
        }
        public List<Koszyk> GetCartItems()
        {
            return storeDB.Koszyks.Where(
                cart => cart.KoszykId == ShoppingCartId).ToList();
        }
        public int GetCount()
        {
            // Get the count of each item in the cart and sum them up
            int? count = (from cartItems in storeDB.Koszyks
                          where cartItems.KoszykId == ShoppingCartId
                          select (int?)cartItems.Count).Sum();
            // Return 0 if all entries are null
            return count ?? 0;
        }
        public decimal GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = (from cartItems in storeDB.Koszyks
                              where cartItems.KoszykId == ShoppingCartId
                              select (int?)cartItems.Count *
                              cartItems.Produkt.Cena).Sum();

            return total ?? decimal.Zero;
        }
       
        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] =
                        context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();
                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        }
        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            var shoppingCart = storeDB.Koszyks.Where(
                c => c.KoszykId == ShoppingCartId);

            foreach (Koszyk item in shoppingCart)
            {
                item.KoszykId = userName;
            }
            storeDB.SaveChanges();
        }
    }
}