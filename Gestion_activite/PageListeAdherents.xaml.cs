using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;

namespace Gestion_activite
{
    public sealed partial class PageListeAdherents : Page
    {
        public ObservableCollection<Adherent> Adherents { get; set; }

        public PageListeAdherents()
        {
            this.InitializeComponent();
            Adherents = new ObservableCollection<Adherent>();
            LoadAdherents();
        }

        // Charger les adhérents depuis la base de données
        private void LoadAdherents()
        {
            try
            {
                var adherentsBDD = SingletonBDD.GetInstance().ObtenirAdherents();
                foreach (var adherent in adherentsBDD)
                {
                    Adherents.Add(new Adherent
                    {
                        ID = adherent["ID"].ToString(),
                        Nom = adherent["Nom"].ToString(),
                        Prenom = adherent["Prenom"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des adhérents : {ex.Message}");
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

        private void ModifierAdherent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Adherent adherent)
            {
                Frame.Navigate(typeof(PageModificationAdherent), adherent);
            }
        }

        private async void SupprimerAdherent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Adherent adherent)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirmation de suppression",
                    Content = $"Êtes-vous sûr de vouloir supprimer {adherent.Prenom} {adherent.Nom} ?",
                    PrimaryButtonText = "Oui",
                    CloseButtonText = "Annuler",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        SingletonBDD.GetInstance().SupprimerAdherent(adherent.ID.ToString());
                        Adherents.Remove(adherent);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Erreur lors de la suppression : {ex.Message}");
                    }
                }
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
    }
}