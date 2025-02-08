using System.ComponentModel.DataAnnotations;

namespace webApiProject.Model
{
    public class ChangePasswordModel
    {
     
        public string OldPassword { get; set; } = string.Empty;

        
        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty; // Pour confirmer le nouveau mot de passe
    }
}
