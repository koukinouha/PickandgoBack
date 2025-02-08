using System;
using System.ComponentModel.DataAnnotations;

namespace webApiProject.Model
{
    public class Colis
    {
        [Key]
        public int Id { get; set; } // Identifiant unique du colis

   
        public string NomComplet { get; set; } = string.Empty; // Nom complet du destinataire

      
        public string CodeGouvernorat { get; set; } = string.Empty; // Code du gouvernorat

 
        public string Delegation { get; set; } = string.Empty; // Délégation

        public string Localite { get; set; } = string.Empty; // Localité

    
        public string Telephone { get; set; } = string.Empty; // Numéro de téléphone

       
        public string Designation { get; set; } = string.Empty; // Désignation du colis

      
        public int NombreArticles { get; set; } // Nombre d'articles dans le colis

      
        public decimal Prix { get; set; } // Prix du colis

        public bool Echange { get; set; } // Indique si le colis peut être échangé

        public string? Commentaire { get; set; } // Commentaires supplémentaires

     
        public string StatutLivraison { get; set; } = string.Empty; // Statut de livraison

        public bool Annulation { get; set; } // Indique si le colis a été annulé

        public DateTime DateEnlevement { get; set; } // Date d'envoi du colis
        public DateTime DateAjoutColis { get; set; } // Date d'envoi du colis
        public DateTime DateRetourColis { get; set; }
        public DateTime remboursment { get; set; }
   
        public int UserId { get; set; } // Clé étrangère pour l'utilisateur
        public User User { get; set; }
    }
}
