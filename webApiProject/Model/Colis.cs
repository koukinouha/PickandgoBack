using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using webApiProject;

public class Colis
{
    [Key]
    public int Id { get; set; } // Identifiant unique du colis
    [JsonProperty(Required = Required.AllowNull)]
    public string? Cin { get; set; }
    public int fraisLivraison { get; set; }
    [JsonProperty(Required = Required.AllowNull)]
    public string? matricule { get; set; }
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
    public bool Annulation { get; set; } // Indique si le colis a été annulée  
    public string DateAjoutColis { get; set; } // Date d'envoi du colis
    public string? ApplicationUserId { get; set; }
    // Clé étrangère pour le colis
   
}

    // Ajoutez cette annotation pour spécifier la clé étrangère
  
    

