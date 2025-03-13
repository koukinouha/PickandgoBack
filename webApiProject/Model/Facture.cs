using System.ComponentModel.DataAnnotations.Schema;

namespace webApiProject.Model
{
    public class Facture
    {
        public int Id { get; set; } // Identifiant unique de la facture
        public string NomDestinataire { get; set; } // Nom complet du destinataire
        public string TelephoneDestinataire { get; set; } // Numéro de téléphone du destinataire
        public string AdresseDestinataire { get; set; } // Adresse du destinataire
        public string NomUtilisateur { get; set; } // Nom complet de l'utilisateur
        public string TelephoneUtilisateur { get; set; } // Numéro de téléphone de l'utilisateur
        public string AdresseUtilisateur { get; set; } // Adresse de l'utilisateur
        public string CodeTVA { get; set; } // Code TVA
        public int NombreColis { get; set; } // Nombre total de colis
        public decimal MontantTotal { get; set; } // Montant total en DT
        public string CodeGouvernorat { get; set; } // Code du gouvernorat
        public string Localite { get; set; } // Localité
        public int ColisId { get; set; }
        [ForeignKey("ColisId")]
        public virtual Colis Colis { get; set; }
    }

}
