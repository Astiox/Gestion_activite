using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    using Microsoft.UI.Xaml.Media;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Gestion_activite
    {
        public class Activite
        {
            public int ID { get; set; }
            public string Nom { get; set; }
            public int TypeActiviteID { get; set; }
            public string Description { get; set; }
            public decimal CoutOrganisation { get; set; }
            public decimal PrixVente { get; set; }
            public decimal MoyenneNotes { get; set; }
            public int NombreParticipants { get; set; }
            public string Image { get; set; }
            public string PrixFormate => $"{PrixVente:F2}$";

        }

    }

}
