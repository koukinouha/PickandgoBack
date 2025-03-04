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

            // Configuration de la relation entre ApplicationUser et Profile
            modelBuilder.Entity<ApplicationUser>()
                   .HasOne(u => u.Profile)
                   .WithOne(p => p.ApplicationUser)
                   .HasForeignKey<Profile>(p => p.ApplicationUserId);

            // Configuration de la relation entre ApplicationUser et Colis
            // Configuration de la relation entre ApplicationUser et Colis
            modelBuilder.Entity<ApplicationUser>()
        .HasMany(u => u.Colis) // Un utilisateur a plusieurs colis
        .WithOne() // Chaque colis appartient à un utilisateur
        .HasForeignKey(c => c.ApplicationUserId) // La clé étrangère dans Colis
        .OnDelete(DeleteBehavior.Restrict); // Comportement de suppression
        }
    }
}
