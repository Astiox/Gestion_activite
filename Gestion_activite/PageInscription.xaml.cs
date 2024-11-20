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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageInscription : Page
    {
        public PageInscription()
        {
            this.InitializeComponent();
        }
        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAccueil));
        }

        private async void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = NomInput.Text;
            string prenom = PrenomInput.Text;
            string email = EmailInput.Text;
            string motDePasse = MotDePasseInput.Password;
            string adresse = AdresseInput.Text;
            DateTime? dateNaissance = DateNaissanceInput.Date.DateTime;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse) ||
                string.IsNullOrWhiteSpace(adresse) || !dateNaissance.HasValue)
            {
                AfficherMessageErreur("Tous les champs doivent être remplis.");
                return;
            }

            try
            {
                SingletonBDD.GetInstance().AjouterAdherent(
                    null, nom, prenom, dateNaissance.Value, adresse, email, motDePasse);

                App.Current.Resources["IsLoggedIn"] = true;
                App.Current.Resources["CurrentUser"] = email;

                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Succès",
                    Content = "Inscription réussie ! Vous êtes maintenant connecté.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();

                Frame.Navigate(typeof(PageAccueil));
            }
            catch (Exception ex)
            {
                AfficherMessageErreur($"Erreur lors de l'inscription : {ex.Message}");
            }
        }

        private void AfficherMessageErreur(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = errorDialog.ShowAsync();
        }




        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }
    }
}
