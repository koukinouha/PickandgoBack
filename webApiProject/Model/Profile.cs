using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webApiProject.Model
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public int Tel { get; set; }
        public int Cin { get; set; }
        public string Patente { get; set; }
        public string Adresse { get; set; }
        public string LanguePrefere { get; set; }
        public string PreferencesCommunication { get; set; }
        public string ModePaiementPrefere { get; set; }
        public string HistoriqueConnexionsActions { get; set; } = "Aucune action de connexion enregistrée";

        // Relation
        public string? ApplicationUserId { get; set; }
        

    }
}