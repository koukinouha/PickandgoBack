using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webApiProject.EmailConfiguration;
using webApiProject.Model;
using webApiProject.Repository;

namespace webApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthRepository _authRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        public AuthController(AuthRepository authRepository, IEmailService emailService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,
        IMemoryCache memoryCache)
        {
            _authRepository = authRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserModel model)
        {
            // Vérifier si l'e-mail existe déjà
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("Cette adresse e-mail est déjà utilisée.");
            }

            // Créer l'utilisateur
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.Phone,
                IsVerified = model.Role == role.Admin, // Seuls les admins sont vérifiés par défaut
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assigner le rôle à l'utilisateur
                await _userManager.AddToRoleAsync(user, model.Role.ToString());

                if (model.Role == role.Admin)
                {
                    // Envoyer un e-mail de confirmation pour les admins
                    await SendAdminConfirmationEmail(model.Email, model.Username, model.Password);
                }
                else if (model.Role == role.Fournisseur)
                {
                    // Envoyer un e-mail à l'administrateur pour informer de la demande d'inscription
                    await SendAdminNotificationEmail(model.Email, model.Username);
                }
                else if (model.Role == role.Client)
                {
                    // Envoyer un e-mail de confirmation pour les clients
                    await SendClientConfirmationEmail(model.Email);
                }

                return Ok("User registered successfully.");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("validateSupplier/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidateSupplier(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            // Vérifier si l'utilisateur est un fournisseur
            if (!await _userManager.IsInRoleAsync(user, role.Fournisseur.ToString()))
            {
                return BadRequest("Cet utilisateur n'est pas un fournisseur.");
            }

            // Activer le compte du fournisseur
            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            // Envoyer un e-mail au fournisseur avec ses informations de connexion
            var password = await _userManager.GeneratePasswordResetTokenAsync(user);
            await SendSupplierConfirmationEmail(user.Email, user.UserName, password);

            return Ok("Le compte du fournisseur a été validé avec succès.");
        }

        [HttpGet("unverifiedSuppliers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnverifiedSuppliers()
        {
            try
            {
                // Récupérer tous les utilisateurs ayant le rôle "Fournisseur"
                var suppliers = await _userManager.GetUsersInRoleAsync(role.Fournisseur.ToString());

                // Filtrer les fournisseurs non validés
                var unverifiedSuppliers = suppliers.Where(user => !user.IsVerified).ToList();

                // Retourner la liste des fournisseurs non validés
                return Ok(unverifiedSuppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur s'est produite lors de la récupération des fournisseurs non validés : {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Vérifier si l'utilisateur est un fournisseur ou un client non vérifié
                if ((await _userManager.IsInRoleAsync(user, role.Fournisseur.ToString()) || await _userManager.IsInRoleAsync(user, role.Client.ToString())) && !user.IsVerified)
                {
                    return BadRequest("Votre compte doit être validé par un administrateur avant de pouvoir vous connecter.");
                }

                // Générer le token JWT
                var tokenString = GenerateJWTToken(user);

                // Retourner le corps de l'utilisateur avec le token
                return Ok(new
                {
                    user = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.IsVerified
                    },
                    token = tokenString
                });
            }

            return Unauthorized("Nom d'utilisateur ou mot de passe incorrect.");
        }

        private async Task<string> GenerateJWTToken(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id), // Claim standard pour l'ID de l'utilisateur
        new Claim("UserId", user.Id), // Claim personnalisé pour l'ID de l'utilisateur
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName)
    };

            // Ajouter les rôles de l'utilisateur
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendAdminConfirmationEmail(string email, string username, string password)
        {
            var subject = "Confirmation d'inscription";
            var message = $"Votre compte administrateur a été créé avec succès.\nNom d'utilisateur : {username}\nMot de passe : {password}";

            await _emailService.SendEmailAsync(email, subject, message);
        }

        private async Task SendAdminNotificationEmail(string email, string username)
        {
            var subject = "Nouvelle demande d'inscription de fournisseur";
            var message = $"Un nouveau fournisseur avec l'e-mail {email} et le nom d'utilisateur {username} a demandé à s'inscrire. Veuillez valider son compte.";

            await _emailService.SendEmailAsync("admin@example.com", subject, message);
        }

        private async Task SendClientConfirmationEmail(string email)
        {
            var subject = "Confirmation d'inscription";
            var message = "Votre compte a été créé avec succès. Veuillez attendre la validation de votre compte.";

            await _emailService.SendEmailAsync(email, subject, message);
        }

        private async Task SendSupplierConfirmationEmail(string email, string username, string password)
        {
            var subject = "Votre compte fournisseur a été validé";
            var message = $"Votre compte fournisseur a été validé par l'administrateur.\nNom d'utilisateur : {username}\nMot de passe : {password}";

            await _emailService.SendEmailAsync(email, subject, message);
        }

        [HttpGet("allusers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersByRole()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Client");
            // Filtrer les utilisateurs ayant isVerified=false
            var unverifiedUsers = usersInRole.Where(user => !user.IsVerified).ToList();
            return Ok(unverifiedUsers);
        }

        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("L'utilisateur spécifié n'a pas été trouvé.");
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok("L'utilisateur a été supprimé avec succès.");
                }
                else
                {
                    return BadRequest("La suppression de l'utilisateur a échoué.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur s'est produite lors de la suppression de l'utilisateur : {ex.Message}");
            }
        }

        [HttpGet("allclients")]
        public async Task<IActionResult> GetAllClients()
        {
            try
            {
                var clients = await _userManager.GetUsersInRoleAsync("Client");
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur s'est produite lors de la récupération des clients : {ex.Message}");
          
            }
        }
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok("Si l'adresse e-mail existe, un code de réinitialisation a été envoyé.");
            }

            var resetCode = GenerateResetCode();

            // Stocker le code dans le cache avec une expiration de 30 minutes
            _memoryCache.Set(model.Email, resetCode, TimeSpan.FromMinutes(30));

            var subject = "Réinitialisation de votre mot de passe";
            var message = $"Votre code de réinitialisation est : {resetCode}";

            await _emailService.SendEmailAsync(model.Email, subject, message);

            return Ok("Si l'adresse e-mail existe, un code de réinitialisation a été envoyé.");
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Une erreur s'est produite lors de la réinitialisation du mot de passe.");
            }

            // Récupérer le code depuis le cache
            if (!_memoryCache.TryGetValue(model.Email, out string storedCode) || storedCode != model.Code)
            {
                return BadRequest("Code de réinitialisation invalide ou expiré.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                // Supprimer le code du cache après utilisation
                _memoryCache.Remove(model.Email);
                return Ok("Votre mot de passe a été réinitialisé avec succès.");
            }

            return BadRequest(result.Errors);
        }
        private string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(10000000, 99999999).ToString(); // Génère un nombre entre 10000000 et 99999999
        }
    }




}