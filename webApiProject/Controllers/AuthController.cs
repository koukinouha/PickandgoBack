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
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { success = -3, message = "Cette adresse e-mail est déjà utilisée." });
            }

            // Vérifier si le nom d'utilisateur existe déjà
            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new { success = -5, message = "Ce nom d'utilisateur est déjà utilisé." });
            }

            // Vérifier si un utilisateur avec le rôle Admin existe déjà
            if (model.Role == role.Admin)
            {
                var existingAdminUser = await _userManager.GetUsersInRoleAsync(role.Admin.ToString());
                if (existingAdminUser.Any())
                {
                    return BadRequest(new { success = -2, message = "Un utilisateur avec le rôle Admin existe déjà." });
                }
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
                    await SendAdminConfirmationEmail(model.Email, model.Username, model.Password);
                }
                else if (model.Role == role.Fournisseur)
                {
                    await SendAdminNotificationEmail(model.Email, model.Username);
                }
                else if (model.Role == role.Assistante)
                {
                    await SendClientConfirmationEmail(model.Email);
                }

                return Ok(new { success = 1, message = "Utilisateur enregistré avec succès." });
            }

            return BadRequest(new { success = -1, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }


        [HttpPost("validateSupplier/{userId}")]
        [Authorize(Roles = "Admin , Assistante")]

        
        public async Task<IActionResult> ValidateSupplier(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new { code = -1, message = "Utilisateur non trouvé." });
                }

                // Vérifier si l'utilisateur est un fournisseur
                if (!await _userManager.IsInRoleAsync(user, role.Fournisseur.ToString()))
                {
                    return Ok(new { code = -1, message = "Cet utilisateur n'est pas un fournisseur." });
                }

                // Activer le compte du fournisseur
                user.IsVerified = true;
                await _userManager.UpdateAsync(user);

                // Générer un token de réinitialisation pour lui envoyer par mail
                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Envoyer l'e-mail
                await SendSupplierConfirmationEmail(user.Email, user.UserName, passwordResetToken);

                return Ok(new { code = 1, message = "Le compte du fournisseur a été validé avec succès." });
            }
            catch (Exception ex)
            {
                return Ok(new { code = -1, message = $"Une erreur est survenue : {ex.Message}" });
            }
        }


        [HttpGet("unverifiedSuppliers")]
        [Authorize(Roles = "Admin,Assistante")]

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
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    // Vérifier si l'utilisateur est un fournisseur non vérifié
                    if (await _userManager.IsInRoleAsync(user, role.Fournisseur.ToString()) && !user.IsVerified)
                    {
                        return BadRequest(new { status = -1, message = "Votre compte doit être validé par un administrateur avant de pouvoir vous connecter." });
                    }

                    // Vérifier si l'utilisateur est un fournisseur validé
                    if (await _userManager.IsInRoleAsync(user, role.Fournisseur.ToString()) && user.IsVerified)
                    {
                        // Ici, vous pouvez ajouter des logiques spécifiques pour les fournisseurs
                    }

                    // Générer le token JWT
                    var tokenString = await GenerateJWTToken(user);

                    // Récupérer le rôle de l'utilisateur
                    var userRole = await GetUserRoleAsync(user);

                    // Retourner le corps de l'utilisateur avec le token et le rôle
                    return Ok(new
                    {
                        status = 5,
                        user = new
                        {
                            user.Id,
                            user.UserName,
                            user.Email,
                            user.FirstName,
                            user.LastName,
                            user.PhoneNumber,
                            user.IsVerified,
                            Role = userRole // Retourner le rôle en tant que chaîne de caractères
                        },
                        token = tokenString
                    });
                }

                return BadRequest(new { status = -1, message = "Nom d'utilisateur ou mot de passe incorrect." });
            }
            catch (Exception ex)
            {
                // Gérer les erreurs
                return StatusCode(500, new { status = -1, message = $"Une erreur s'est produite lors de la connexion : {ex.Message}" });
            }
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

            // Ajouter le rôle de l'utilisateur
            var role = await GetUserRoleAsync(user);
            claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GetUserRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(role.Fournisseur.ToString()))
            {
                return role.Fournisseur.ToString();
            }
            else if (roles.Contains(role.Admin.ToString()))
            {
                return role.Admin.ToString();
            }
            else if (roles.Contains(role.Assistante.ToString()))
            {
                return role.Assistante.ToString();
            }
            else
            {
                // Si aucun rôle n'est trouvé, retourner une chaîne vide
                return "";
            }
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
            var message = $" Bonjour Mr /Mme : {username} Votre compte fournisseur a été validé par l'administrateur.Bienvenu avec nous Pick And Go .";

            await _emailService.SendEmailAsync(email, subject, message);
        }

        [HttpGet("allFournisseur")]
        [Authorize(Roles = "Admin")]
       
        public async Task<IActionResult> GetAllUsersByRole()
        {
            // Récupérer tous les utilisateurs ayant le rôle "Fournisseur"
            var usersInRole = await _userManager.GetUsersInRoleAsync("Fournisseur");

            // Retourner la liste complète des fournisseurs
            return Ok(usersInRole);
        }


        [HttpDelete("deleteUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { succes = -1, message = "L'utilisateur spécifié n'a pas été trouvé." });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok(new { succes = 1, message = "L'utilisateur a été supprimé avec succès." });
                }
                else
                {
                    return BadRequest(new { succes = -1, message = "La suppression de l'utilisateur a échoué." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { succes = -1, message = $"Une erreur s'est produite lors de la suppression de l'utilisateur : {ex.Message}" });
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



        [HttpGet("logout")]
        public IActionResult Logout()
        {
            try
            {
                // Récupérer le token JWT depuis les en-têtes de la requête
                var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    // Pas de token fourni, retourner un code d'échec
                    return Ok(new { Status = -1, Message = "Aucun token JWT n'a été fourni." });
                }

                // Désactiver le token JWT
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);

                // Créer un nouveau jeton avec une date d'expiration passée
                var newToken = new JwtSecurityToken(
                    jwtToken.Issuer,
                    jwtToken.Audiences.FirstOrDefault(),
                    jwtToken.Claims,
                    DateTime.UtcNow.AddMinutes(-1),
                    jwtToken.ValidTo,
                    jwtToken.SigningCredentials
                );

                // Générer le nouveau token expiré
                var expiredToken = jwtHandler.WriteToken(newToken);

                // Retourner un code de succès
                return Ok(new { Status = 1, Message = "Déconnexion réussie.", ExpiredToken = expiredToken });
            }
            catch (Exception ex)
            {
                // Gérer les erreurs, retourner un code d'échec
                return Ok(new { Status = -1, Message = $"Une erreur s'est produite lors de la déconnexion : {ex.Message}" });
            }
        }
        [HttpGet("currentUser")]
        [Authorize] // Nécessite que l'utilisateur soit authentifié
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Récupérer l'ID de l'utilisateur à partir des claims (ici UserId)
                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Identifiant utilisateur introuvable dans les claims.");
                }

                // Récupérer l'utilisateur à partir de l'ID
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Utilisateur non trouvé.");
                }

                // Récupérer le rôle de l'utilisateur
                var role = await GetUserRoleAsync(user);

                // Retourner les informations de l'utilisateur
                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.IsVerified,
                    Role = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur s'est produite lors de la récupération de l'utilisateur : {ex.Message}");
            }
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