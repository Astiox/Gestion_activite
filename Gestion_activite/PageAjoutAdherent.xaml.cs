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
    public sealed partial class PageAjoutAdherent : Page
    {
        public PageAjoutAdherent()
        {
            this.InitializeComponent();
        }

        private void AjouterAdherentButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = NomInput.Text.Trim();
            string prenom = PrenomInput.Text.Trim();
            DateTime? dateNaissance = DateNaissanceInput.SelectedDate?.DateTime;
            string adresse = AdresseInput.Text.Trim();
            string email = EmailInput.Text.Trim();
            string motDePasse = PasswordInput.Password.Trim();
            string confirmMotDePasse = ConfirmPasswordInput.Password.Trim();

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom) ||
                !dateNaissance.HasValue || string.IsNullOrWhiteSpace(adresse) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse) ||
                string.IsNullOrWhiteSpace(confirmMotDePasse))
            {
                ShowErrorMessage("Tous les champs doivent être remplis.");
                return;
            }

            if (motDePasse != confirmMotDePasse)
            {
                ShowErrorMessage("Les mots de passe ne correspondent pas.");
                return;
            }

            if (SingletonBDD.GetInstance().EmailExiste(email))
            {
                ShowErrorMessage("Cet email est déjà utilisé.");
                return;
            }

            try
            {
                SingletonBDD.GetInstance().AjouterAdherent(Guid.NewGuid().ToString(), nom, prenom,
                    dateNaissance.Value, adresse, motDePasse, email, DateTime.Now);
                ShowSuccessMessage("Adhérent ajouté avec succès !");
                Frame.Navigate(typeof(PageType));
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Une erreur est survenue : {ex.Message}");
            }
        }


        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private async void ShowErrorMessage(string message)
        {
            await new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }

        private async void ShowSuccessMessage(string message)
        {
            await new ContentDialog
            {
                Title = "Succès",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
    }
}