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
    public sealed partial class PageAjoutAdministrateur : Page
    {
        public PageAjoutAdministrateur()
        {
            this.InitializeComponent();
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void AjouterAdministrateurButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = NomInput.Text;
            string prenom = PrenomInput.Text;
            string email = EmailInput.Text;
            string motDePasse = MotDePasseInput.Password;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
            {
                ShowErrorMessage("Veuillez remplir tous les champs.");
                return;
            }

            if (SingletonBDD.GetInstance().EmailAdminExiste(email))
            {
                ShowErrorMessage("Un administrateur avec cet email existe déjà.");
                return;
            }

            try
            {
                SingletonBDD.GetInstance().AjouterAdministrateur(nom, prenom, email, motDePasse);

                ShowSuccessMessage("Administrateur ajouté avec succès !");
                Frame.Navigate(typeof(PageType));
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors de l'ajout : {ex.Message}");
            }
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