using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSolutionEFCoreMVC.Data
{
    public class EcommerceDbContext : IdentityDbContext<ApplicationUser>
    {
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relacionamento entre ApplicationUser e Address (via chave estrangeira)
            builder.Entity<Address>()
                .HasOne(a => a.ApplicationUser)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);            

            // Configurar relação entre ApplicationUser e Position
            builder.Entity<ApplicationUser>()
                .HasOne(e => e.Position)
                .WithMany(p => p.ApplicationUser)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Cascade);            

            // Configurar relação entre ShoppingCart e ApplicationUser
            builder.Entity<ShoppingCart>()
                .HasOne(sc => sc.ApplicationUser)
                .WithOne(c => c.ShoppingCart)
                .HasForeignKey<ShoppingCart>(sc => sc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação entre Order e ApplicationUser
            builder.Entity<Order>()
                .HasOne(o => o.ApplicationUser)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação entre OrderItem e Order
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação entre OrderItem e Product
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação entre OrderItem e Product
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação entre ShoppingCart e ShoppingCartItem
            builder.Entity<ShoppingCartItem>()
                .HasOne(sci => sci.ShoppingCart)
                .WithMany(sc => sc.ShoppingCartItems)
                .HasForeignKey(sci => sci.ShoppingCartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relação entre Product e ShoppingCartItem
            builder.Entity<ShoppingCartItem>()
                .HasOne(sci => sci.Product)
                .WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(sci => sci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relação entre Order e Address
            builder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
