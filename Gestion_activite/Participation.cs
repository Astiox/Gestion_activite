using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    internal class Participation
    {
        
            public int ID { get; set; }
            public string AdherentID { get; set; }
            public int SeanceID { get; set; }
            public decimal? Note { get; set; }
        


        public Participation() { }

        public Participation(int id, string adherentID, int seanceID, decimal? note)
        {
            ID = id;
            AdherentID = adherentID;
            SeanceID = seanceID;
            Note = note;
        }
    }
}
