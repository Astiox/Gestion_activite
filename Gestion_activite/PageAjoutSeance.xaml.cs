using Gestion_activite.Gestion_activite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Gestion_activite
{
    public sealed partial class PageAjoutSeance : Page
    {
        public ObservableCollection<Activite> Activites { get; set; }

        public PageAjoutSeance()
        {
            this.InitializeComponent();
            Activites = new ObservableCollection<Activite>();
            LoadActivites();
        }

        private void LoadActivites()
        {
            try
            {
                var activitesBDD = SingletonBDD.GetInstance().ObtenirActivites();
                foreach (var activite in activitesBDD)
                {
                    Activites.Add(new Activite
                    {
                        ID = Convert.ToInt32(activite["ID"]),
                        Nom = activite["Nom"].ToString()
                    });
                }
                ActiviteComboBox.ItemsSource = Activites;
                ActiviteComboBox.DisplayMemberPath = "Nom";
                ActiviteComboBox.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des activités : {ex.Message}");
            }
        }

        private void AjouterSeanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiviteComboBox.SelectedItem == null ||
                DateInput.SelectedDate == null ||
                HoraireInput.SelectedTime == null ||
                string.IsNullOrWhiteSpace(PlacesTotalesInput.Text) ||
                !int.TryParse(PlacesTotalesInput.Text, out int placesTotales))
            {
                ShowErrorMessage("Veuillez remplir tous les champs correctement.");
                return;
            }

            try
            {
                int activiteID = (int)ActiviteComboBox.SelectedValue;
                DateTime date = DateInput.SelectedDate.Value.Date;
                TimeSpan horaire = HoraireInput.SelectedTime.Value;
                SingletonBDD.GetInstance().AjouterSeance(activiteID, date, horaire, placesTotales);

                ShowSuccessMessage("Séance ajoutée avec succès !");
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
