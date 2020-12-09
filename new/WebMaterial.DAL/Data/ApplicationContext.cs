using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMaterial.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebMaterial.DAL.Data
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Material> Materials { get; set; }
        public DbSet<Version> Versions { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Version>()
                .HasOne(p => p.Material)
                .WithMany(b => b.Versions)
                .HasForeignKey(l => l.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }
    }
}
