using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webApiProject.Model;

namespace webApiProject
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Colis> Colis { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId);
            modelBuilder.Entity<Colis>()
                .HasOne(c => c.User)
                .WithMany(u => u.Colis)
                .HasForeignKey(c => c.UserId);
        }
    }
}