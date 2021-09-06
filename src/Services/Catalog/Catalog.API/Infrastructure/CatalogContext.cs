using eShopLabs.Services.Catalog.API.Infrastructure.EntityConfigurations;
using eShopLabs.Services.Catalog.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace eShopLabs.Services.Catalog.API.Infrastructure
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }
        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<CatalogBrand> CatalogBrands { get; set; }
        public DbSet<CatalogType> CatalogTypes { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
            builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
            builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
        }
    }

    // public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    // {
    //     public CatalogContext CreateDbContext(string[] args)
    //     {
    //         var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
    //             .UseSqlServer("Server=.;Initial Catalog=eShopLabs.Services.CatalogDb;Integrated Security=true");

    //         return new CatalogContext(optionsBuilder.Options);
    //     }
    // }
}
