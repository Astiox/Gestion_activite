using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    
        public class TypeActivite
        {
            public int ID { get; set; } 
            public string Nom { get; set; } 
            public string Description { get; set; }
            public string Image { get; set; }

        public bool IsAdmin => SingletonBDD.GetUtilisateurConnecte()?["Role"].ToString() == "Admin";

        public TypeActivite() { }

            public TypeActivite(int id, string nom, string description,string image)
            {
                ID = id;
                Nom = nom;
                Description = description;
                Image = image;
            }

           
        }
}

