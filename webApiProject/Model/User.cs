using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace webApiProject.Model
{
    public class User 
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Adresse { get; set; } = string.Empty;
        public role? role { get; set; }
      
        public Profile Profile { get; set; } // Relation un utilisateur a un seul profil
        public ICollection<Colis> Colis { get; set; } = new List<Colis>();
    }
}
