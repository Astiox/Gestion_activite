using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    internal class Categorie
    {
        public int ID { get; set; }
        public string NomCategorie { get; set; }
        public string Description { get; set; }

        public Categorie() { }

        public Categorie(int id, string nomCategorie, string description)
        {
            ID = id;
            NomCategorie = nomCategorie; 
            Description = description;
        }

        public override string ToString()
        {
            return $"{NomCategorie} : {Description}";
        }
    }
}
