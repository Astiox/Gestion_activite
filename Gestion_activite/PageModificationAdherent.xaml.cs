using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Gestion_activite
{
    public sealed partial class PageModificationAdherent : Page
    {
        private Adherent AdherentAModifier;

        public PageModificationAdherent()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Adherent adherent)
            {
                AdherentAModifier = adherent;
                NomInput.Text = adherent.Nom;
                PrenomInput.Text = adherent.Prenom;
                AdresseInput.Text = adherent.Adresse;
                DateNaissanceInput.Date = adherent.DateNaissance;
                DateInscriptionInput.Date = adherent.DateInscription;
            }
        }

        private void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            AdherentAModifier.Nom = NomInput.Text;
            AdherentAModifier.Prenom = PrenomInput.Text;
            AdherentAModifier.Adresse = AdresseInput.Text;
            AdherentAModifier.DateNaissance = DateNaissanceInput.Date.DateTime;
            AdherentAModifier.DateInscription = DateInscriptionInput.Date.DateTime;

            SingletonBDD.GetInstance().ModifierAdherent(
                AdherentAModifier.ID,
                AdherentAModifier.Nom,
                AdherentAModifier.Prenom,
                AdherentAModifier.DateNaissance,
                AdherentAModifier.Adresse,
                AdherentAModifier.DateInscription);

            Frame.Navigate(typeof(PageListeAdherents));
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageListeAdherents));
        }
    }
}
