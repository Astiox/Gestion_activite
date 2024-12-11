using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    public class Seance
    {
        public int ID { get; set; }
        public int ActiviteID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Horaire { get; set; }
        public int PlacesRestantes { get; set; }
        public int PlacesTotales { get; set; }
        public decimal PrixUnitaire { get; set; }

        public string FormattedDate => Date.ToString("dd/MM/yyyy");
        public string FormattedHoraire => Horaire.ToString(@"hh\:mm");
        public string PlacesRestantesText => PlacesRestantes > 0 ? $"Places restantes : {PlacesRestantes}" : "Complet";
        public SolidColorBrush PlacesRestantesColor => PlacesRestantes > 0
            ? new SolidColorBrush(Microsoft.UI.Colors.Green)
            : new SolidColorBrush(Microsoft.UI.Colors.Red);

        public bool EstComplete => PlacesRestantes <= 0;

        public Seance() { }

        public Seance(int id, int activiteID, DateTime date, TimeSpan horaire, int placesRestantes, int placesTotales, decimal prixUnitaire)
        {
            ID = id;
            ActiviteID = activiteID;
            Date = date;
            Horaire = horaire;
            PlacesRestantes = placesRestantes;
            PlacesTotales = placesTotales;
            PrixUnitaire = prixUnitaire;
        }

        public override string ToString()
        {
            return $"{FormattedDate} à {FormattedHoraire} - {PlacesRestantesText} - {PrixUnitaire:C}";
        }
    }
}
