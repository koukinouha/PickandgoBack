using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using webApiProject.Model;

namespace webApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ColisController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }


        // GET: api/Colis/filtrer/nomecomplet/{nomComplet}
        [HttpGet("filtrer/nomecomplet/{nomComplet}")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisByNomComplet(string nomComplet)
        {
            var colisList = await _context.Colis
                .Where(c => c.NomComplet.Contains(nomComplet))
                .ToListAsync();

            if (colisList == null || !colisList.Any())
            {
                return NotFound("Aucun colis trouvé avec ce nom complet.");
            }

            return Ok(colisList);
        }

        // GET: api/Colis/filtrer/delegationlocalite/{delegationLocalite}
        [HttpGet("filtrer/delegationlocalite/{delegationLocalite}")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisByDelegationLocalite(string delegationLocalite)
        {
            var colisList = await _context.Colis
                .Where(c => c.Delegation.Contains(delegationLocalite))
                .ToListAsync();

            if (colisList == null || !colisList.Any())
            {
                return NotFound("Aucun colis trouvé pour cette délégation/localité.");
            }

            return Ok(colisList);
        }

        // GET: api/Colis/filtrer/dateajout/{dateAjout}
        [HttpGet("filtrer/dateajout/{dateAjout}")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisByDateAjout(DateTime dateAjout)
        {
            var colisList = await _context.Colis
                .Where(c => c.DateAjoutColis.Date == dateAjout.Date)
                .ToListAsync();

            if (colisList == null || !colisList.Any())
            {
                return NotFound("Aucun colis trouvé pour cette date d'ajout.");
            }

            return Ok(colisList);
        }
        // GET: api/Colis/total
        [HttpGet("Nombre Totale des colis")]
        public async Task<ActionResult<int>> GetnumberTotalColis()
        {
            try
            {
                int totalColis = await _context.Colis.CountAsync();
                return Ok(totalColis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("nombre-total-colis-utilisateur-connecte")]
        [Authorize(Roles = "Fournisseur")]// Assurez-vous que l'utilisateur est authentifié
        public async Task<ActionResult<int>> GetNombreTotalColisUtilisateurConnecte()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var utilisateurId = await GetUserIdFromTokenAsync();

                // Compter le nombre de colis pour l'utilisateur spécifié
                int totalColis = await _context.Colis.CountAsync(c => c.ApplicationUserId == utilisateurId);
                return Ok(totalColis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("nombre-total-colis/{utilisateurId}")]
        [Authorize(Roles = "Admin")] // Assurez-vous que seuls les utilisateurs autorisés peuvent accéder à cette méthode
        public async Task<ActionResult<int>> GetNombreTotalColisParUtilisateur(string utilisateurId)
        {
            try
            {
                // Compter le nombre de colis pour l'utilisateur spécifié
                int totalColis = await _context.Colis.CountAsync(c => c.ApplicationUserId == utilisateurId);
                return Ok(totalColis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        // GET: api/Colis/total-echange
        [HttpGet("total colis en echange")]
        public async Task<ActionResult<int>> GetTotalColisEchange()
        {
            try
            {
                int totalColisEchange = await _context.Colis.CountAsync(c => c.Echange);
                return Ok(totalColisEchange);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("total-colis-echange-utilisateur-connecte")]
        [Authorize(Roles = "Fournisseur")]// Assurez-vous que l'utilisateur est authentifié
        public async Task<ActionResult<int>> GetTotalColisEchangeUtilisateurConnecte()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var utilisateurId = await GetUserIdFromTokenAsync();

                // Compter le nombre de colis en échange pour l'utilisateur spécifié
                int totalColisEchange = await _context.Colis.CountAsync(c => c.ApplicationUserId == utilisateurId && c.Echange);
                return Ok(totalColisEchange);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("total-colis-echange/{utilisateurId}")]
        [Authorize(Roles = "Admin")] // Assurez-vous que seuls les utilisateurs autorisés peuvent accéder à cette méthode
        public async Task<ActionResult<int>> GetTotalColisEchangeParUtilisateur(string utilisateurId)
        {
            try
            {
                // Compter le nombre de colis en échange pour l'utilisateur spécifié
                int totalColisEchange = await _context.Colis.CountAsync(c => c.ApplicationUserId == utilisateurId && c.Echange);
                return Ok(totalColisEchange);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        // GET: api/Colis/colis-echange
        [HttpGet("liste-colis-echange")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisEchange()
        {
            try
            {
                var colisEchangeList = await _context.Colis
                    .Where(c => c.Echange)
                    .ToListAsync();

                if (colisEchangeList == null || !colisEchangeList.Any())
                {
                    return NotFound("Aucun colis trouvé avec l'échange activé.");
                }

                return Ok(colisEchangeList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        // GET: api/Colis/total-livres/{annee}/{mois}
        [HttpGet("total-livres/{annee}/{mois}")]
        public async Task<ActionResult<int>> GetTotalColisLivres(int annee, int mois)
        {
            try
            {
                // Vérification de la validité du mois
                if (mois < 1 || mois > 12)
                {
                    return BadRequest("Le mois doit être compris entre 1 et 12.");
                }

                // Calcul des dates de début et de fin pour le mois donné
                var dateDebut = new DateTime(annee, mois, 1);
                var dateFin = dateDebut.AddMonths(1).AddDays(-1); // Dernier jour du mois

                // Comptage des colis livrés dans le mois spécifié
                int totalColisLivres = await _context.Colis
                    .CountAsync(c => c.StatutLivraison == "Livré" && c.DateAjoutColis >= dateDebut && c.DateAjoutColis <= dateFin);

                return Ok(totalColisLivres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        // GET: api/Colis/livres
        [HttpGet("Get Total Colis Livres")]
        public async Task<ActionResult<int>> GetTotalColisLivres()
        {
            try
            {
                int totalLivres = await _context.Colis.CountAsync(c => c.StatutLivraison == "Livré");
                return Ok(totalLivres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        // GET: api/Colis/en-cours-de-traitement
        [HttpGet("totale colis en-cours-de-traitement")]
        public async Task<ActionResult<int>> GetTotalColisEnCoursDeTraitement()
        {
            try
            {
                int totalEnCoursDeTraitement = await _context.Colis.CountAsync(c => c.StatutLivraison == "EnCoursDeTraitement");
                return Ok(totalEnCoursDeTraitement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("total-colis-en-cours-de-traitement-for-fournissuer concted")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<int>> GetTotalColisEnCoursDeTraitementforFournissuerconcted()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var fournisseurId = await GetUserIdFromTokenAsync();

                // Compter le nombre de colis en cours de traitement pour le fournisseur spécifié
                int totalEnCoursDeTraitement = await _context.Colis.CountAsync(c => c.ApplicationUserId == fournisseurId && c.StatutLivraison == "EnCoursDeTraitement");
                return Ok(totalEnCoursDeTraitement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("total-colis-en-cours-de-traitement/{fournisseurId}")]
        [Authorize(Roles = "Admin")] // Assurez-vous que seuls les utilisateurs autorisés peuvent accéder à cette méthode
        public async Task<ActionResult<int>> GetTotalColisEnCoursDeTraitementByFournisseur(string fournisseurId)
        {
            try
            {
                // Compter le nombre de colis en cours de traitement pour le fournisseur spécifié
                int totalEnCoursDeTraitement = await _context.Colis.CountAsync(c => c.ApplicationUserId == fournisseurId && c.StatutLivraison == "EnCoursDeTraitement");
                return Ok(totalEnCoursDeTraitement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        // GET: api/Colis/annules
        [HttpGet("Get Total Colis Annules")]
        public async Task<ActionResult<int>> GetTotalColisAnnules()
        {
            try
            {
                int totalAnnules = await _context.Colis.CountAsync(c => c.StatutLivraison == "Annulé");
                return Ok(totalAnnules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        [HttpGet("Get Total Colis Annules Fournisseur Conncted")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<int>> GetTotalColisAnnulesFournisseur()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var fournisseurId = await GetUserIdFromTokenAsync();

                // Compter le nombre de colis annulés pour le fournisseur spécifié
                int totalAnnules = await _context.Colis.CountAsync(c => c.ApplicationUserId == fournisseurId && c.StatutLivraison == "Annulé");
                return Ok(totalAnnules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("GetTotalColisAnnulesByFournisseur/{fournisseurId}")]
        public async Task<ActionResult<int>> GetTotalColisAnnulesByFournisseur(string fournisseurId)
        {
            try
            {
                // Compter le nombre de colis annulés pour le fournisseur spécifié
                int totalAnnules = await _context.Colis.CountAsync(c => c.ApplicationUserId == fournisseurId && c.StatutLivraison == "Annulé");
                return Ok(totalAnnules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        // GET: api/Colis
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColis()
        {
            return await _context.Colis.ToListAsync();
        }

        // GET: api/Colis/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Colis>> GetColis(int id)
        {
            var colis = await _context.Colis.FindAsync(id);
            if (colis == null)
            {
                return NotFound();
            }
            return colis;
        }
        [HttpPost("ajouter")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<IActionResult> AjouterColis([FromBody] Colis colis)
        {
            if (colis == null)
            {
                return BadRequest("Le colis ne peut pas être nul.");
            }

            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();
                colis.ApplicationUserId = userId; // Assigner l'ID de l'utilisateur

                colis.DateAjoutColis = DateTime.UtcNow;
                colis.StatutLivraison = "EnCoursDeTraitement"; // Définir le statut de livraison

                _context.Colis.Add(colis);
                await _context.SaveChangesAsync();

                return Ok(new { ColisId = colis.Id });
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur interne s'est produite.");
            }
        }








        // PUT: api/Colis/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutColis(int id, Colis colis)
        {
            if (id != colis.Id)
            {
                return BadRequest();
            }

            _context.Entry(colis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ColisExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }
        // GET: api/Colis/mes-colis
        [HttpGet("mes-colis")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisByFournisseur()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Récupérer les colis associés à cet ID utilisateur
                var colisList = await _context.Colis
                    .Where(c => c.ApplicationUserId == userId)
                    .ToListAsync();

                if (colisList == null || !colisList.Any())
                {
                    return NotFound("Aucun colis trouvé pour ce fournisseur.");
                }

                return Ok(colisList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        // DELETE: api/Colis/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColis(int id)
        {
            var colis = await _context.Colis.FindAsync(id);
            if (colis == null)
            {
                return NotFound();
            }

            _context.Colis.Remove(colis);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Colis/{id}/UpdateStatus
        [HttpPut("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdateColisStatus(int id, [FromBody] string newStatus)
        {
            var colis = await _context.Colis.FindAsync(id);
            if (colis == null)
            {
                return NotFound();
            }

            colis.StatutLivraison = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [HttpPost("ajouter-colis-liste")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<IActionResult> AjouterColisliste([FromBody] Colis[] colis)
        {
            if (colis == null || colis.Length == 0)
            {
                return BadRequest("Aucun colis à ajouter.");
            }

            var userId = await GetUserIdFromTokenAsync();
            var addedColisIds = new List<int>();

            foreach (Colis col in colis)
            {
                col.ApplicationUserId = userId; // Assigner l'ID de l'utilisateur
                col.DateAjoutColis = DateTime.UtcNow;

                try
                {
                    _context.Colis.Add(col);
                    await _context.SaveChangesAsync();

                    addedColisIds.Add(col.Id); // Ajouter l'ID du colis ajouté à la liste
                }
                catch (ApplicationException appEx)
                {
                    // Gérer les exceptions ApplicationException
                    return NotFound(appEx.Message);
                }
                catch (DbUpdateException dbEx)
                {
                    // Gérer les exceptions DbUpdateException
                    return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur s'est produite lors de l'ajout du colis.");
                }
                catch (Exception ex)
                {
                    // Gérer les autres exceptions
                    return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur interne s'est produite.");
                }
            }

            return Ok(new { ColisIds = addedColisIds }); // Retourner tous les IDs des colis ajoutés
        }

        // Journaliser l'erreur

        // GET: api/Colis/GetUserId
        [Authorize]
        [HttpGet("GetUserId")]
        public async Task<ActionResult<string>> GetUserId()
        {
            try
            {
                var userId = await GetUserIdFromTokenAsync();
                return Ok(new { UserId = userId });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        [HttpGet("total-colis-livres-montant for fournisseur concté")]
        [Authorize(Roles = "Fournisseur")]
       
        public async Task<IActionResult> GetTotalColisLivresMontant()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Récupérer les colis livrés pour cet utilisateur
                var colisLivres = await _context.Colis
                    .Where(c => c.ApplicationUserId == userId && c.StatutLivraison == "Livré")
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return NotFound($"Aucun colis livré trouvé pour l'utilisateur {userId}.");
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = colisLivres.Count,
                    MontantTotal = montantTotal
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        [HttpGet("le total des colis livrés pour tous les utilisateurs")]
        public async Task<IActionResult> GetTotalColisLivresMontantallUSERS()
        {
            try
            {
                // Récupérer tous les colis livrés
                var colisLivres = await _context.Colis
                    .Where(c => c.StatutLivraison == "Livré")
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return NotFound("Aucun colis livré trouvé.");
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = colisLivres.Count,
                    MontantTotal = montantTotal
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }



        [HttpGet("total des colis livrés pour un fournisseur spécifique /{fournisseurId}")]
        public async Task<IActionResult> GetTotalColisLivresMontantByFournisseur(string fournisseurId)
        {
            try
            {
                // Récupérer les colis livrés pour le fournisseur spécifié
                var colisLivres = await _context.Colis
                    .Where(c => c.ApplicationUserId == fournisseurId && c.StatutLivraison == "Livré")
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return NotFound($"Aucun colis livré trouvé pour le fournisseur avec l'ID {fournisseurId}.");
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = colisLivres.Count,
                    MontantTotal = montantTotal
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        [HttpGet("total-colis-livres-montant-this-month for Fournisseur Conncted")]
        [Authorize(Roles = "Fournisseur")]

        public async Task<IActionResult> GetTotalColisLivresMontantThisMonth()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Récupérer le mois et l'année en cours
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                // Récupérer les colis livrés pour cet utilisateur et le mois en cours
                var colisLivres = await _context.Colis
                    .Where(c => c.ApplicationUserId == userId
                            && c.StatutLivraison == "Livré"
                            && c.DateAjoutColis.Month == currentMonth
                            && c.DateAjoutColis.Year == currentYear)
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return Ok(new
                    {
                        TotalColisLivres = -1,
                        MontantTotal = -1
                    });
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = 1,
                    MontantTotal = montantTotal
                });
            }
            catch (ApplicationException ex)
            {
                return Ok(new
                {
                    TotalColisLivres = -1,
                    MontantTotal = -1
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    TotalColisLivres = -1,
                    MontantTotal = -1
                });
            }
        }
        [HttpGet("total-colis-livres-montant-all-users-this-month")]
        public async Task<IActionResult> GetTotalColisLivresMontantAllUsersThisMonth()
        {
            try
            {
                // Récupérer le mois et l'année en cours
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                // Récupérer tous les colis livrés pour le mois en cours
                var colisLivres = await _context.Colis
                    .Where(c => c.StatutLivraison == "Livré"
                            && c.DateAjoutColis.Month == currentMonth
                            && c.DateAjoutColis.Year == currentYear)
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return Ok(new
                    {
                        TotalColisLivres = 0,
                        MontantTotal = 0
                    });
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = colisLivres.Count,
                    MontantTotal = montantTotal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpGet("total-colis-livres-montant-by-fournisseur/{fournisseurId}")]
        public async Task<IActionResult> GetTotalColisLivresMontantByFournisseurThisMonth(string fournisseurId)
        {
            try
            {
                // Récupérer le mois et l'année en cours
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                // Récupérer les colis livrés pour le fournisseur spécifié et le mois en cours
                var colisLivres = await _context.Colis
                    .Where(c => c.ApplicationUserId == fournisseurId
                            && c.StatutLivraison == "Livré"
                            && c.DateAjoutColis.Month == currentMonth
                            && c.DateAjoutColis.Year == currentYear)
                    .ToListAsync();

                if (colisLivres == null || !colisLivres.Any())
                {
                    return Ok(new
                    {
                        TotalColisLivres = 0,
                        MontantTotal = 0
                    });
                }

                // Calculer le montant total des colis livrés
                var montantTotal = colisLivres.Sum(c => c.Prix);

                return Ok(new
                {
                    TotalColisLivres = colisLivres.Count,
                    MontantTotal = montantTotal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        // Méthode privée pour récupérer l'ID de l'utilisateur à partir du token
        private async Task<string> GetUserIdFromTokenAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new ApplicationException("L'ID de l'utilisateur n'a pas été trouvé dans le token JWT.");
            }

            // Vérifier que l'utilisateur existe dans la base de données
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Utilisateur avec l'ID '{userId}' introuvable.");
            }

            return userId;
        }

        private bool ColisExists(int id)
        {
            return _context.Colis.Any(e => e.Id == id);
        }


        [HttpGet("paginated")]
        public async Task<ActionResult<PagedResult<Colis>>> GetColisPaginated(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalColis = await _context.Colis.CountAsync();
                var colis = await _context.Colis
                    .OrderBy(c => c.Id) // Assurez-vous de trier pour une pagination correcte
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<Colis>
                {
                    Items = colis,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalColis,
                    TotalPages = (int)Math.Ceiling((double)totalColis / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
    }
}
    // GET: api/Colis



  



