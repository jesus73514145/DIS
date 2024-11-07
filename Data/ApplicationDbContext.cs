using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyecto.Models;


namespace proyecto.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Costeo> DataCosteo { get; set; }
    public DbSet<Material> DataMaterial { get; set; }
    public DbSet<Prenda> DataPrenda { get; set; }
    public DbSet<Sedes> DataSedes { get; set; }

}
