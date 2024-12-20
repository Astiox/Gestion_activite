﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    public class Adherent
    {
        public string ID { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Adresse { get; set; }
        public DateTime DateInscription { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }

        public Adherent() { }

        public Adherent(string id, string nom, string prenom, DateTime dateNaissance, string adresse, DateTime dateInscription, string email, string motDePasse)
        {
            ID = id;
            Nom = nom;
            Prenom = prenom;
            DateNaissance = dateNaissance;
            Adresse = adresse;
            DateInscription = dateInscription;
            Email = email;
            MotDePasse = motDePasse;
        }
    }
}
