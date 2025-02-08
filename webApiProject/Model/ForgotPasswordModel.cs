using System.ComponentModel.DataAnnotations;

namespace webApiProject.Model
{
    public class ForgotPasswordModel
    {
        
        public string Email { get; set; } = string.Empty;
    }
}
