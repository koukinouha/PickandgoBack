using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using webApiProject.Model;

namespace webApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // GET: api/Profile
        [HttpGet("liste des profile")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await _context.Profiles.ToListAsync();
        }

        // GET: api/Profile/5
        [HttpGet("afficher_profile/{id}")]
        public async Task<ActionResult<Profile>> GetProfile(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        // PUT: api/Profile/5
        [HttpPut("modifier_profile/{id}")]
        public async Task<IActionResult> PutProfile(int id, Profile profile)
        {
            if (id != profile.Id)
            {
                return BadRequest();
            }

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
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

        [HttpPost("ajouter-profile")]
        [Authorize]
        public async Task<IActionResult> AjouterProfile([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest("Le profil ne peut pas être nul.");
            }

            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Vérifier si un profil existe déjà pour cet utilisateur
                var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
                if (existingProfile != null)
                {
                    return BadRequest("Un profil existe déjà pour cet utilisateur.");
                }

                // Assigner l'ID de l'utilisateur au profil
                profile.ApplicationUserId = userId;

                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();

                return Ok(new { ProfileId = profile.Id });
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

        [HttpGet("Me")]
        [Authorize]
        public async Task<ActionResult<Profile>> GetMyProfile()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir du token
                var userId = await GetUserIdFromTokenAsync();

                // Récupérer le profil associé à l'utilisateur connecté
                var profile = await _context.Profiles
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

                if (profile == null)
                {
                    return NotFound("Profil non trouvé pour cet utilisateur.");
                }

                return profile;
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

        // DELETE: api/Profile/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfileExists(int id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }

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
    }
}
