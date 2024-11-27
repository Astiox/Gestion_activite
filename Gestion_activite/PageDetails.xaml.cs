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
using System.Globalization;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageDetails : Page
    {
        public Activite SelectedActivite { get; set; }
        public ObservableCollection<string> DatesDisponibles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<HoraireSelection> HorairesDisponibles { get; set; } = new ObservableCollection<HoraireSelection>();

        public PageDetails()
        {
            this.InitializeComponent();
            DataContext = this;
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

            if (SelectedActivite != null)
            {
                LoadDatesDisponibles();
            }
        }

        private void LoadDatesDisponibles()
        {
            DatesDisponibles.Clear();
            var dates = SingletonBDD.GetInstance().GetAvailableDates(SelectedActivite.ID);

            foreach (var date in dates)
            {
                DatesDisponibles.Add(date.ToString("dd/MM/yyyy"));
            }

            DatesDisponiblesComboBox.ItemsSource = DatesDisponibles;
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAccueil));
        }

        private void DatesDisponiblesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatesDisponiblesComboBox.SelectedItem is string selectedDateString &&
                DateTime.TryParseExact(selectedDateString, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime selectedDate))
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
                if (seance.PlacesRestantes > 0)
                {
                    HorairesDisponibles.Add(new HoraireSelection
                    {
                        Horaire = seance.Horaire.ToString(@"hh\:mm"),
                        IsSelected = false
                    });
                }
            }

            HorairesList.ItemsSource = HorairesDisponibles;
        }

        private async void ConfirmerButton_Click(object sender, RoutedEventArgs e)
        {
            var utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
            if (utilisateurConnecte == null)
            {
                await DemanderConnexionOuInscriptionAsync();
                utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
                if (utilisateurConnecte == null)
                {
                    return; // L'utilisateur n'est toujours pas connecté
                }
            }

            try
            {
                var horairesSelectionnes = HorairesDisponibles.FirstOrDefault(h => h.IsSelected);
                if (horairesSelectionnes == null)
                {
                    await new ContentDialog
                    {
                        Title = "Erreur",
                        Content = "Veuillez sélectionner un horaire.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }

                var seanceCorrespondante = SingletonBDD.GetInstance()
                    .GetSeances(SelectedActivite.ID,
                                DateTime.ParseExact((string)DatesDisponiblesComboBox.SelectedItem, "dd/MM/yyyy", null))
                    .FirstOrDefault(s => s.Horaire.ToString(@"hh\:mm") == horairesSelectionnes.Horaire);

                if (seanceCorrespondante != null)
                {
                    int adherentID = (int)utilisateurConnecte["ID"]; 
                    SingletonBDD.GetInstance().ReserverPlace(seanceCorrespondante.ID);
                    SingletonBDD.GetInstance().AjouterParticipation(adherentID, seanceCorrespondante.ID, null);

                    HorairesDisponibles.Remove(horairesSelectionnes);
                }

                await new ContentDialog
                {
                    Title = "Réservation confirmée",
                    Content = $"Votre participation pour l'horaire {horairesSelectionnes.Horaire} a été enregistrée.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();

                LoadHorairesDisponibles(DateTime.ParseExact((string)DatesDisponiblesComboBox.SelectedItem, "dd/MM/yyyy", null));
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Erreur",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }



        private async Task DemanderConnexionOuInscriptionAsync()
        {
            var choixDialog = new ContentDialog
            {
                Title = "Connexion requise",
                Content = "Vous devez être connecté pour confirmer votre participation. Que souhaitez-vous faire ?",
                PrimaryButtonText = "Se connecter",
                SecondaryButtonText = "S'inscrire",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            var result = await choixDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DemanderConnexionAsync();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Frame.Navigate(typeof(PageInscription));
            }
        }

        private async Task DemanderConnexionAsync()
        {
            var emailInput = new TextBox { PlaceholderText = "Email" };
            var passwordInput = new PasswordBox { PlaceholderText = "Mot de passe" };
            var errorMessage = new TextBlock
            {
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var loginContent = new StackPanel
            {
                Children = { emailInput, passwordInput, errorMessage }
            };

            var loginDialog = new ContentDialog
            {
                Title = "Connexion",
                Content = loginContent,
                PrimaryButtonText = "Connexion",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            loginDialog.PrimaryButtonClick += (sender, args) =>
            {
                string email = emailInput.Text;
                string password = passwordInput.Password;

                if (!SingletonBDD.GetInstance().EmailExiste(email) || !SingletonBDD.GetInstance().VerifierConnexion(email, password))
                {
                    errorMessage.Text = "Adresse email ou mot de passe incorrect.";
                    errorMessage.Visibility = Visibility.Visible;
                    args.Cancel = true;
                }
                else
                {
                    var utilisateur = SingletonBDD.GetInstance().AuthentifierUtilisateur(email, password);
                    SingletonBDD.SetUtilisateurConnecte(utilisateur); 
                }
            };

            await loginDialog.ShowAsync();
        }

    }
}





