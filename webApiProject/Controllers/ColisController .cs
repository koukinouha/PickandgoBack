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

            // Vérifier que le nombre d'articles est au moins 1
            if (colis.NombreArticles < 1)
            {
                return BadRequest("Le nombre d'articles doit être au moins 1.");
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


        [HttpGet("Get_Total_Colis_Retour_Nombre")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisRetourNombre()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, calculer le nombre total de tous les colis avec le statut "Retour à l'expéditeur"
                    var totalNombre = await _context.Colis
                        .Where(c => c.StatutLivraison == "RetourÀLExpéditeur")
                        .CountAsync();
                    return Ok(totalNombre);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, calculer le nombre total des colis avec le statut "Retour à l'expéditeur" pour ce fournisseur
                    var totalNombreFournisseur = await _context.Colis
                        .Where(c => c.ApplicationUserId == userId && c.StatutLivraison == "RetourÀLExpéditeur")
                        .CountAsync();
                    return Ok(totalNombreFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        [HttpPut("ModifierStatutLivraison/{colisId}")]
        [Authorize(Roles = "Admin,Assistante")]
        public async Task<IActionResult> ModifierStatutLivraison(int colisId, [FromBody] string nouveauStatut)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier si l'utilisateur a le rôle "Admin" ou "Assistante"
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));
                if (!roles.Contains("Admin") && !roles.Contains("Assistante"))
                {
                    return Forbid("Accès interdit : seuls les administrateurs et les assistantes peuvent modifier le statut de livraison.");
                }

                // Récupérer le colis à modifier
                var colis = await _context.Colis.FindAsync(colisId);
                if (colis == null)
                {
                    return NotFound($"Le colis avec l'ID {colisId} n'a pas été trouvé.");
                }

                // Mettre à jour le statut de livraison
                colis.StatutLivraison = nouveauStatut;
                _context.Colis.Update(colis);
                await _context.SaveChangesAsync();

                return Ok($"Le statut de livraison du colis {colisId} a été mis à jour avec le nouveau statut : {nouveauStatut}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        [HttpGet("Get_Total_Colis_Annule_Nombre")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisAnnuleNombre()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, calculer le nombre total de tous les colis avec le statut "Annulé"
                    var totalNombre = await _context.Colis
                        .Where(c => c.StatutLivraison == "Annulé")
                        .CountAsync();
                    return Ok(totalNombre);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, calculer le nombre total des colis avec le statut "Annulé" pour ce fournisseur
                    var totalNombreFournisseur = await _context.Colis
                        .Where(c => c.ApplicationUserId == userId && c.StatutLivraison == "Annulé")
                        .CountAsync();
                    return Ok(totalNombreFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
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


        // DELETE: api/Colis/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColis(int id)
        {
            try
            {
                var colis = await _context.Colis.FindAsync(id);
                if (colis == null)
                {
                    return Ok(new { code = -1, message = "Colis introuvable." });
                }

                _context.Colis.Remove(colis);
                await _context.SaveChangesAsync();

                return Ok(new { code = 1, message = "Colis supprimé avec succès." });
            }
            catch (Exception ex)
            {
                return Ok(new { code = -1, message = $"Erreur lors de la suppression du colis : {ex.Message}" });
            }
        }





        [HttpPost("ajouter_colis_liste")]
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








        [HttpGet("Get_Total_Colis_Livres_Montant")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<decimal>> GetTotalColisLivresMontant()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, calculer le montant total de tous les colis livrés
                    var totalMontant = await _context.Colis
                        .Where(c => c.StatutLivraison == "Livré")
                        .SumAsync(c => c.Prix * c.NombreArticles);
                    return Ok(totalMontant);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, calculer le montant total des colis livrés par ce fournisseur
                    var totalMontantFournisseur = await _context.Colis
                        .Where(c => c.ApplicationUserId == userId && c.StatutLivraison == "Livré")
                        .SumAsync(c => c.Prix * c.NombreArticles);
                    return Ok(totalMontantFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }







        [HttpGet("Get_TotalNumber_Colis")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColis()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, compter tous les colis
                    int totalColis = await _context.Colis.CountAsync();
                    return Ok(totalColis);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, compter les colis ajoutés par ce fournisseur
                    int totalColisFournisseur = await _context.Colis.CountAsync(c => c.ApplicationUserId == userId);
                    return Ok(totalColisFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
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




        // GET: api/Colis/total-echange
        [HttpGet("Get_Total_Colis_Echange")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisEchange()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, compter tous les colis en échange
                    int totalColisEchange = await _context.Colis.CountAsync(c => c.Echange);
                    return Ok(totalColisEchange);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, compter les colis en échange pour ce fournisseur
                    int totalColisEchangeFournisseur = await _context.Colis.CountAsync(c => c.ApplicationUserId == userId && c.Echange);
                    return Ok(totalColisEchangeFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        // GET: api/Colis/colis-echange
        [HttpGet("liste_colis_echange")]
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
        [HttpGet("total_livres/{annee}/{mois}")]
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
        [HttpGet("Get_Total_Colis_En_Cours_De_Traitement")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisEnCoursDeTraitement()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, compter tous les colis en cours de traitement
                    int totalEnCours = await _context.Colis.CountAsync(c => c.StatutLivraison == "EnCoursDeTraitement");
                    return Ok(totalEnCours);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, compter les colis en cours de traitement pour ce fournisseur
                    int totalEnCoursFournisseur = await _context.Colis.CountAsync(c => c.ApplicationUserId == userId && c.StatutLivraison == "EnCoursDeTraitement");
                    return Ok(totalEnCoursFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        // GET: api/Colis/annules
        [HttpGet("Get_Total_Colis_Annules")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisAnnules()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, compter tous les colis annulés
                    int totalAnnules = await _context.Colis.CountAsync(c => c.StatutLivraison == "Annulé");
                    return Ok(totalAnnules);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, compter les colis annulés pour ce fournisseur
                    int totalAnnulesFournisseur = await _context.Colis.CountAsync(c => c.ApplicationUserId == userId && c.StatutLivraison == "Annulé");
                    return Ok(totalAnnulesFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
        [HttpGet("Get_Total_Colis_Livres")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<int>> GetTotalColisLivres()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, compter tous les colis livrés
                    int totalLivres = await _context.Colis.CountAsync(c => c.StatutLivraison == "Livré");
                    return Ok(totalLivres);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, compter les colis livrés pour ce fournisseur
                    int totalLivresFournisseur = await _context.Colis.CountAsync(c => c.ApplicationUserId == userId && c.StatutLivraison == "Livré");
                    return Ok(totalLivresFournisseur);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        // GET: api/Colis/liste
        [HttpGet("mes-colis")]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisByRole()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier le rôle de l'utilisateur
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

                if (roles.Contains("Admin") || roles.Contains("Assistante"))
                {
                    // Si l'utilisateur est Admin ou Assistante, retourner tous les colis
                    var colisList = await _context.Colis.ToListAsync();
                    return Ok(colisList);
                }
                else if (roles.Contains("Fournisseur"))
                {
                    // Si l'utilisateur est Fournisseur, retourner les colis ajoutés par ce fournisseur
                    var colisList = await _context.Colis
                        .Where(c => c.ApplicationUserId == userId)
                        .ToListAsync();

                    if (colisList == null || !colisList.Any())
                    {
                        return NotFound("Aucun colis trouvé pour ce fournisseur.");
                    }

                    return Ok(colisList);
                }

                return Forbid("Accès interdit : rôle non autorisé.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /** [HttpGet("GetTotalColisAnnulesByFournisseur/{fournisseurId}")]
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
         }**/

    }
}
    // GET: api/Colis



  



