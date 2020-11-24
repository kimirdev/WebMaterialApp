using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMaterialApp.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Version> Versions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Material>(i => {
                i.HasKey(v => v.Id);
                i.HasMany(v => v.Versions)
                .WithOne(v => v.Material)
                .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Version>(i => {
                i.HasKey(v => v.Id);
                i.HasOne(v => v.Material)
                .WithMany(v => v.Versions);
            });
        }
    }
}
