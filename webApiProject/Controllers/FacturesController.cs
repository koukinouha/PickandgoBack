using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

using webApiProject.Model;

namespace webApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacturesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Factures/generer/{colisId}
        // GET: api/Factures/generer/{colisId}
        [HttpGet("generer/{colisId}")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<Facture>> GenererFacture(int colisId)
        {
            try
            {
                // Récupérer les informations du colis
                var colis = await _context.Colis
                    .FirstOrDefaultAsync(c => c.Id == colisId);

                if (colis == null)
                    return NotFound("Colis non trouvé.");

                // Récupérer les informations de l'utilisateur
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == colis.ApplicationUserId);

                if (user == null)
                    return BadRequest("Utilisateur non trouvé.");

                // Créer la facture
                var facture = new Facture
                {
                    NomDestinataire = colis.NomComplet,
                    TelephoneDestinataire = colis.Telephone,
                    AdresseDestinataire = colis.Delegation,
                    CodeTVA = "%",
                    NombreColis = 1, // Utiliser le nombre d'articles du colis
                    MontantTotal = colis.Prix * colis.NombreArticles,
                 
                    CodeGouvernorat = colis.CodeGouvernorat,
                    Localite = colis.Localite,
                    ColisId = colis.Id,
                    NomUtilisateur = $"{user.LastName} {user.FirstName}",
                    AdresseUtilisateur = colis.Delegation,
                    TelephoneUtilisateur = colis.Telephone // Remplir la propriété TelephoneUtilisateur
                };

                _context.Factures.Add(facture);
                await _context.SaveChangesAsync();

                return Ok(facture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        [HttpGet("GetFactureForUser/{userId}")]
        [Authorize(Roles = "Fournisseur, Admin")]
        public async Task<ActionResult<Facture>> GetFactureForUser(string userId)
        {
            try
            {
                // Récupérer les informations de l'utilisateur
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return BadRequest("Utilisateur non trouvé.");

                // Récupérer tous les colis liés à cet utilisateur
                var colis = await _context.Colis
                    .Where(c => c.ApplicationUserId == userId)
                    .ToListAsync();

                // Calculer le nombre total de colis
                int totalColis = colis.Count;


                // Calculer le montant total
                decimal montantTotal = colis.Sum(c => c.Prix * c.NombreArticles);

                // Créer la facture
                var facture = new Facture
                {
                    NomDestinataire = "", // Ne pas afficher
                    TelephoneDestinataire = "", // Ne pas afficher
                    AdresseDestinataire = "", // Ne pas afficher
                    CodeTVA = "%",
                    NombreColis = totalColis,
                    MontantTotal = montantTotal,
                    CodeGouvernorat = "", // Ne pas afficher
                    Localite = "",
                    ColisId = 0, // Ne pas afficher
                    NomUtilisateur = user.LastName + " " + user.FirstName,
                    AdresseUtilisateur = "", // Ne pas afficher
                    TelephoneUtilisateur = user.FirstName
                };

                return Ok(facture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        [HttpPut("update/{colisId}")]
        [Authorize(Roles = "Fournisseur, Admin")]
        public async Task<IActionResult> UpdateFacture(int colisId, [FromBody] Facture updatedFacture)
        {
            try
            {
                // Récupérer la facture existante
                var existingFacture = await _context.Factures.FirstOrDefaultAsync(f => f.ColisId == colisId);

                if (existingFacture == null)
                    return NotFound($"Aucune facture trouvée pour le colis {colisId}.");

                // Mettre à jour les champs de la facture
                existingFacture.NomDestinataire = updatedFacture.NomDestinataire;
                existingFacture.TelephoneDestinataire = updatedFacture.TelephoneDestinataire;
                existingFacture.AdresseDestinataire = updatedFacture.AdresseDestinataire;
                existingFacture.CodeTVA = updatedFacture.CodeTVA;
                existingFacture.NombreColis = updatedFacture.NombreColis;
                existingFacture.MontantTotal = updatedFacture.MontantTotal;
                existingFacture.CodeGouvernorat = updatedFacture.CodeGouvernorat;
                existingFacture.Localite = updatedFacture.Localite;
                existingFacture.NomUtilisateur = updatedFacture.NomUtilisateur;
                existingFacture.AdresseUtilisateur = updatedFacture.AdresseUtilisateur;
                existingFacture.TelephoneUtilisateur = updatedFacture.TelephoneUtilisateur;

                // Enregistrer les modifications dans la base de données
                await _context.SaveChangesAsync();

                return Ok(existingFacture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        // GET: api/Factures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFactures()
        {
            return await _context.Factures.ToListAsync();
        }

        // GET: api/Factures/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Facture>> GetFacture(int id)
        {
            var facture = await _context.Factures.FindAsync(id);

            if (facture == null)
            {
                return NotFound();
            }

            return facture;
        }

        // DELETE: api/Factures/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Assurez-vous que seuls les admins peuvent supprimer des factures
        public async Task<IActionResult> DeleteFacture(int id)
        {
            var facture = await _context.Factures.FindAsync(id);
            if (facture == null)
            {
                return NotFound();
            }

            _context.Factures.Remove(facture);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
