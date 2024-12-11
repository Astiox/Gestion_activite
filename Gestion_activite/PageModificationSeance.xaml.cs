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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Gestion_activite
{
    public sealed partial class PageModificationSeance : Page
    {
        public ObservableCollection<Seance> Seances { get; set; }

        public PageModificationSeance()
        {
            this.InitializeComponent();
            Seances = new ObservableCollection<Seance>();
            LoadSeances();
        }

        private void LoadSeances()
        {
            try
            {
                var seancesBDD = SingletonBDD.GetInstance().ObtenirSeances();
                SeanceComboBox.ItemsSource = seancesBDD;
                SeanceComboBox.DisplayMemberPath = "Nom";
                SeanceComboBox.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des séances : {ex.Message}");
            }
        }

        private void SeanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeanceComboBox.SelectedItem is Seance selectedSeance)
            {
                DateInput.SelectedDate = selectedSeance.Date;
                HoraireInput.Time = selectedSeance.Horaire;
                PlacesTotalesInput.Text = selectedSeance.PlacesTotales.ToString();

                foreach (var item in ActiviteComboBox.Items)
                {
                    if (item is Activite activite && activite.ID == selectedSeance.ActiviteID)
                    {
                        ActiviteComboBox.SelectedItem = activite;
                        break;
                    }
                }
            }
        }

        private void ModifierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SeanceComboBox.SelectedItem is Seance selectedSeance &&
                ActiviteComboBox.SelectedItem is Activite selectedActivite)
            {
                try
                {
                    DateTime selectedDate = DateInput.SelectedDate.HasValue
                        ? DateInput.SelectedDate.Value.DateTime
                        : throw new InvalidOperationException("Veuillez sélectionner une date valide.");

                    SingletonBDD.GetInstance().ModifierSeance(
                        selectedSeance.ID,
                        selectedActivite.ID,
                        selectedDate,
                        HoraireInput.Time,
                        int.Parse(PlacesTotalesInput.Text));

                    ShowSuccessMessage("Séance modifiée avec succès !");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Erreur lors de la modification : {ex.Message}");
                }
            }
            else
            {
                ShowErrorMessage("Veuillez sélectionner une séance et une activité.");
            }
        }






        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private async Task ShowDialogAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            if (this.XamlRoot != null)
            {
                dialog.XamlRoot = this.XamlRoot;
            }

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'affichage du ContentDialog : {ex.Message}");
            }
        }

        private async void ShowErrorMessage(string message)
        {
            await ShowDialogAsync("Erreur", message);
        }

        private async void ShowSuccessMessage(string message)
        {
            await ShowDialogAsync("Succès", message);
        }

    }
}