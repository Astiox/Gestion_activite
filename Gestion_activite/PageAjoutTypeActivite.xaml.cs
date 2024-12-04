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
    public sealed partial class PageAjoutTypeActivite : Page
    {
        public PageAjoutTypeActivite()
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

        private void AjouterTypeActiviteButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = NomTypeInput.Text;
            string description = DescriptionInput.Text;
            string imageUrl = ImageUrlInput.Text;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl))
            {
                ShowErrorMessage("Veuillez remplir tous les champs.");
                return;
            }

            try
            {
                SingletonBDD.GetInstance().AjouterTypeActivite(new TypeActivite
                {
                    Nom = nom,
                    Description = description,
                    Image = imageUrl
                });

                ShowSuccessMessage("Type d'activité ajouté avec succès !");
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
