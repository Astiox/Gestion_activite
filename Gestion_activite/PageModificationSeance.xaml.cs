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
        public ObservableCollection<Activite> Activites { get; set; }

        public PageModificationSeance()
        {
            this.InitializeComponent();
            Seances = new ObservableCollection<Seance>();
            Activites = new ObservableCollection<Activite>();
            LoadSeances();
            LoadActivites();
        }

        private void LoadSeances()
        {
            try
            {
                var seancesBDD = SingletonBDD.GetInstance().ObtenirSeances();
                Seances.Clear();
                foreach (var seance in seancesBDD)
                {
                    Seances.Add(new Seance
                    {
                        ID = Convert.ToInt32(seance["ID"]),
                        ActiviteID = Convert.ToInt32(seance["ActiviteID"]),
                        Date = Convert.ToDateTime(seance["Date"]),
                        Horaire = TimeSpan.Parse(seance["Horaire"].ToString()),
                        PlacesTotales = Convert.ToInt32(seance["PlacesTotales"])
                    });
                }
                SeanceComboBox.ItemsSource = Seances;
                SeanceComboBox.DisplayMemberPath = "ID"; 
                SeanceComboBox.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des séances : {ex.Message}");
            }
        }

        private void LoadActivites()
        {
            try
            {
                var activitesBDD = SingletonBDD.GetInstance().Obteniractivites();
                Activites.Clear();
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
                        ? DateInput.SelectedDate.Value.Date
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

        private async void ShowErrorMessage(string message)
        {
            if (this.XamlRoot == null)
            {
                Console.WriteLine("Erreur : XamlRoot est null. Le ContentDialog ne peut pas être affiché.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot 
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Erreur lors de l'affichage du ContentDialog : {ex.Message}");
            }
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

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }
    }
}