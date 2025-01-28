using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
           

            public AuthController(AuthRepository authRepository, IEmailService emailService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
            {
                _authRepository = authRepository;
                _userManager = userManager;
                _roleManager = roleManager;
                _configuration = configuration;
                _emailService = emailService;
                
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

                bool isAdmin = model.IsAdmin; // Supposons que vous récupériez cette information du formulaire d'inscription

                var result = await _authRepository.RegisterUserAsync(model);
                if (result.Succeeded)
                {
                    if (isAdmin)
                    {
                        // Envoyer un e-mail avec le nom d'utilisateur et le mot de passe pour les administrateurs
                        var password = model.Password; // Assurez-vous de récupérer le mot de passe de manière sécurisée
                        await SendAdminConfirmationEmail(model.Email, model.Username, password);
                    }
                    else
                    {
                        // Envoyer un e-mail de confirmation pour les clients
                        await SendClientConfirmationEmail(model.Email);
                    }

                    return Ok("User registered successfully.");
                }
                return BadRequest(result.Errors);
            }

            private async Task SendAdminConfirmationEmail(string email, string username, string password)
            {
                var subject = "Confirmation d'inscription";
                var message = $"Votre compte administrateur a été créé avec succès.\nNom d'utilisateur : {username}\nMot de passe : {password}";

                await _emailService.SendEmailAsync(email, subject, message);
            }

            private async Task SendClientConfirmationEmail(string email)
            {
                var subject = "Confirmation d'inscription";
                var message = "Votre compte a été créé avec succès. Veuillez attendre la validation de votre compte.";

                await _emailService.SendEmailAsync(email, subject, message);
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login(LoginModel model)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (!user.IsVerified && await _userManager.IsInRoleAsync(user, "Client"))
                    {
                        return BadRequest("Votre compte doit être vérifié avant de pouvoir vous connecter.");
                    }

                    var tokenString = GenerateJWTToken(user);
                    return Ok(new { token = tokenString });
                }
                return Unauthorized();
            }

            private async Task<string> GenerateJWTToken(ApplicationUser user)
            {
                var jwtKey = _configuration["Jwt:Key"];
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
    {
        // Revendications standard
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id), // Ajouter l'identifiant de l'utilisateur
    
          new Claim("UserId", user.Id),
        // Ajouter d'autres informations de l'utilisateur
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName)
    };

                // Récupérer les rôles de l'utilisateur
                var roles = await _userManager.GetRolesAsync(user);

                // Ajouter chaque rôle en tant que revendication
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

        }

    }


