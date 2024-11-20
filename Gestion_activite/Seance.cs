using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    internal class Seance
    {
        public int ID { get; set; }
        public int ActiviteID { get; set; }
        public DateTime Date { get; set; }
        public string Horaire { get; set; }
        public int PlacesRestantes { get; set; }
        public int PlacesTotales { get; set; }

        public string PlacesRestantesText => PlacesRestantes > 0 ? $"Places restantes : {PlacesRestantes}" : "Complet";
        public SolidColorBrush PlacesRestantesColor => PlacesRestantes > 0
            ? new SolidColorBrush(Microsoft.UI.Colors.Green)
            : new SolidColorBrush(Microsoft.UI.Colors.Red);

        public Seance() { }

        public Seance(int id, int activiteID, DateTime dateSeance, int placesRestantes, int placesTotales, string horaire)
        {
            ID = id;
            ActiviteID = activiteID;
            Date = dateSeance;
            PlacesRestantes = placesRestantes;
            PlacesTotales = placesTotales;
            Horaire = horaire;
        }
    }
}
