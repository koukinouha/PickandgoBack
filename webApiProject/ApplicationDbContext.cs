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
        public DbSet<Facture> Factures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation entre ApplicationUser et Profile
            modelBuilder.Entity<Profile>()
                .HasOne<ApplicationUser>() // Pas de propriété de navigation dans Profile
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.ApplicationUserId) // La clé étrangère
                .IsRequired(false) // Si vous souhaitez que cette relation soit optionnelle
                .OnDelete(DeleteBehavior.Restrict); // Comportement de suppression
            modelBuilder.Entity<Facture>()
               .HasOne(f => f.Colis)
               .WithMany()
               .HasForeignKey(f => f.ColisId)
               .OnDelete(DeleteBehavior.Cascade);
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
