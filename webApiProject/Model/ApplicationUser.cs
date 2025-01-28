﻿
using Microsoft.AspNetCore.Identity;
using webApiProject.Model;

namespace webApiProject
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsVerified { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Relation avec le profil de l'utilisateur
        public virtual Profile Profile { get; set; }


        // Relation avec les courriers de l'utilisateur


        public static implicit operator ApplicationUser(User v)
        {
            throw new NotImplementedException();
        }
    }
}