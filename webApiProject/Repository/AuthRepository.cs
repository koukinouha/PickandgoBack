using Microsoft.AspNetCore.Identity;
using webApiProject.Model;

namespace webApiProject.Repository
{
    public class AuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Ajout de RoleManager

        public AuthRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager) // Injection de RoleManager
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager; // Initialisation de RoleManager
        }

        public async Task<IdentityResult> RegisterUserAsync(CreateUserModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsVerified = false, // Par défaut, l'utilisateur n'est pas vérifié
                Role = model.Role // Attribuer le rôle directement
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Vérifier si le rôle existe déjà
                var roleExists = await _roleManager.RoleExistsAsync(model.Role.ToString());
                if (!roleExists)
                {
                    // Créer le rôle s'il n'existe pas
                    await _roleManager.CreateAsync(new IdentityRole(model.Role.ToString()));
                }

                // Attribuer le rôle à l'utilisateur
                await _userManager.AddToRoleAsync(user, model.Role.ToString());
            }

            return result;
        }
        public async Task<SignInResult> LoginAsync(LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
            return result;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersByRole(role selectedRole)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(selectedRole.ToString());
            return usersInRole;
        }
    }
}

