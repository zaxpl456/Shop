using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shop.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentitySample.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        static ApplicationDbContext()
        {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new Kategoria.Mapping());
        }

        public System.Data.Entity.DbSet<Shop.Models.Kategoria> Kategorias { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Adres> Adres { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Dostawa> Dostawas { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Produkt> Produkts { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Producent> Producents { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Profil> Profil { get; set; }
        public System.Data.Entity.DbSet<Shop.Models.Koszyk> Koszyks { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.Zamowienie> Zamowienies { get; set; }

        public System.Data.Entity.DbSet<Shop.Models.ZamowioneSztuki> ZamowioneSztukis  { get; set; }





    }

}
