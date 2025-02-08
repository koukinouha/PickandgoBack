using System.ComponentModel.DataAnnotations;

namespace webApiProject.Model
{
    public class ResetPasswordModel
    {
       
        public string Email { get; set; } = string.Empty;


        public string Code { get; set; }


        public string NewPassword { get; set; } = string.Empty;

      
        public string ConfirmPassword { get; set; } = string.Empty; // Pour confirmer le nouveau mot de passe
    }
}
