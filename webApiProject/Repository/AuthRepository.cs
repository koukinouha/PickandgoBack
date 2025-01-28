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
                IsVerified = false // Par défaut, l'utilisateur n'est pas vérifié
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Attribuer un rôle à l'utilisateur
                if (model.IsAdmin)
                {
                    // Vérifier si le rôle "Admin" existe déjà
                    var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
                    if (!adminRoleExists)
                    {
                        // Créer le rôle "Admin" s'il n'existe pas
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // Vérifier si le rôle "Client" existe déjà
                    var clientRoleExists = await _roleManager.RoleExistsAsync("Client");
                    if (!clientRoleExists)
                    {
                        // Créer le rôle "Client" s'il n'existe pas
                        await _roleManager.CreateAsync(new IdentityRole("Client"));
                    }

                    await _userManager.AddToRoleAsync(user, "Client");
                }
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

