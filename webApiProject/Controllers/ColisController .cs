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

        public ColisController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpPost]
        public async Task<ActionResult<Colis>> PostColis(Colis colis)
        {
            // Initialiser la date d'ajout du colis à la date d'aujourd'hui
            colis.DateAjoutColis = DateTime.Now;

            // Initialiser le statut de livraison par défaut à "En attente"
            colis.StatutLivraison = "En attente";

            // Initialiser l'annulation par défaut à "false"
            colis.Annulation = false;

            // Récupérer l'ID de l'utilisateur connecté
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            // Convertir l'ID de l'utilisateur en int
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("L'ID de l'utilisateur est invalide.");
            }

            // Assigner l'ID de l'utilisateur au colis
            colis.UserId = userId; // Assurez-vous que la propriété UserId existe dans votre modèle Colis

            _context.Colis.Add(colis);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetColis", new { id = colis.Id }, colis);
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
                else
                {
                    throw;
                }
            }

            return NoContent();
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

        private bool ColisExists(int id)
        {
            return _context.Colis.Any(e => e.Id == id);
        }

        // PUT: api/Colis/{id}/UpdateStatus
        [HttpPut("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdateColisStatus(int id, string newStatus)
        {
            var colis = await _context.Colis.FindAsync(id);
            if (colis == null)
            {
                return NotFound();
            }

            // Mettre à jour le statut du colis
            colis.StatutLivraison = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("MyColis")]
        public async Task<ActionResult<IEnumerable<Colis>>> GetColisForCurrentUser()
        {
            // Récupérer l'ID de l'utilisateur connecté
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            // Convertir l'ID de l'utilisateur en int
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("L'ID de l'utilisateur est invalide.");
            }

            // Récupérer les colis associés à l'utilisateur connecté
            var colisList = await _context.Colis
                .Where(c => c.UserId == userId) // Filtrer par UserId (int)
                .ToListAsync();

            return colisList;
        }

    }

}
