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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageDetails : Page
    {
        public Activite SelectedActivite { get; set; }
        public ObservableCollection<DateTime> DatesDisponibles { get; set; } = new ObservableCollection<DateTime>();
        public ObservableCollection<HoraireSelection> HorairesDisponibles { get; set; } = new ObservableCollection<HoraireSelection>();

        public bool IsAnyHoraireSelected => HorairesDisponibles.Any(h => h.IsSelected);

        public PageDetails()
        {
            this.InitializeComponent();
            DatesDisponiblesComboBox.ItemsSource = SingletonBDD.GetInstance().getDateSeance();
        }
        public class HoraireSelection
        {
            public string Horaire { get; set; }
            public bool IsSelected { get; set; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SelectedActivite = e.Parameter as Activite;
            DataContext = this;

            //LoadDatesDisponibles();
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAccueil));
        }

        //private void LoadDatesDisponibles()
        //{
        //    var seances = SingletonBDD.GetInstance().GetSeances(SelectedActivite.ID);
        //    var dates = seances.Select(s => s.Date.Date).Distinct();

        //    DatesDisponibles.Clear();
        //    foreach (var date in dates)
        //    {
        //        DatesDisponibles.Add(date);
        //    }
        //}

        private void DatesDisponiblesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatesDisponiblesComboBox.SelectedItem is DateTime selectedDate)
            {
                LoadHorairesDisponibles(selectedDate);
            }
        }

        private void LoadHorairesDisponibles(DateTime date)
        {
            HorairesDisponibles.Clear();

            var seances = SingletonBDD.GetInstance().GetSeances(SelectedActivite.ID, date);

            foreach (var seance in seances)
            {
                HorairesDisponibles.Add(new HoraireSelection
                {
                    Horaire = seance.Horaire,
                    IsSelected = false
                });
            }

            HorairesList.ItemsSource = HorairesDisponibles;

            ConfirmerButton.IsEnabled = IsAnyHoraireSelected;
        }

        private async void ConfirmerButton_Click(object sender, RoutedEventArgs e)
        {
            if (SingletonBDD.UtilisateurConnecte == null)
            {
                ContentDialog actionDialog = new ContentDialog
                {
                    Title = "Connexion requise",
                    Content = "Vous devez être connecté pour confirmer votre participation. Que souhaitez-vous faire ?",
                    PrimaryButtonText = "Se connecter",
                    SecondaryButtonText = "S'inscrire",
                    CloseButtonText = "Annuler",
                    XamlRoot = this.XamlRoot
                };

                actionDialog.PrimaryButtonClick += (dlgSender, dlgArgs) =>
                {
                };

                actionDialog.SecondaryButtonClick += (dlgSender, dlgArgs) =>
                {
                    Frame.Navigate(typeof(PageInscription));
                };

                var actionResult = await actionDialog.ShowAsync();
                if (SingletonBDD.UtilisateurConnecte == null)
                {
                    return;
                }
            }

            try
            {
                var horairesConfirmee = HorairesDisponibles.Where(h => h.IsSelected).Select(h => h.Horaire).ToList();

                foreach (var horaire in horairesConfirmee)
                {
                    var seance = SingletonBDD.GetInstance().GetSeances(SelectedActivite.ID, (DateTime)DatesDisponiblesComboBox.SelectedItem)
                                              .FirstOrDefault(s => s.Horaire == horaire);

                    if (seance != null)
                    {
                        SingletonBDD.GetInstance().ReserverPlace(seance.ID);
                    }
                }

                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Réservation confirmée",
                    Content = "Votre participation a été confirmée pour les horaires sélectionnés.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();

                LoadHorairesDisponibles((DateTime)DatesDisponiblesComboBox.SelectedItem);
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Erreur",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
