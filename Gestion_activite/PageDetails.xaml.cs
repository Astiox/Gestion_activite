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
        public ObservableCollection<string> HorairesDisponibles { get; set; } = new ObservableCollection<string>();
        public string HoraireSelectionne { get; set; }


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

        private void Logo_Click(object sender, PointerRoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
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

            if (dates == null || dates.Count == 0)
            {
                Console.WriteLine($"Aucune date disponible pour l'activité {SelectedActivite.ID}");
                DatesDisponiblesComboBox.ItemsSource = null;
                return;
            }

            foreach (var date in dates)
            {
                DatesDisponibles.Add(date.ToString("dd/MM/yyyy"));
            }

            DatesDisponiblesComboBox.ItemsSource = DatesDisponibles;
            Console.WriteLine($"Dates disponibles pour l'activité {SelectedActivite.ID} : {string.Join(", ", DatesDisponibles)}");
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

            if (seances == null || seances.Count == 0)
            {
                Console.WriteLine($"Aucune séance trouvée pour l'activité {SelectedActivite.ID} à la date {date:dd/MM/yyyy}");
                HorairesComboBox.ItemsSource = null;
                return;
            }

            foreach (var seance in seances)
            {
                if (seance.PlacesRestantes > 0)
                {
                    HorairesDisponibles.Add(seance.Horaire.ToString(@"hh\:mm"));
                }
            }

            HorairesComboBox.ItemsSource = HorairesDisponibles;

            if (HorairesDisponibles.Count == 0)
            {
                Console.WriteLine($"Aucun horaire disponible pour l'activité {SelectedActivite.ID} à la date {date:dd/MM/yyyy}");
            }
            else
            {
                Console.WriteLine($"Horaires disponibles pour l'activité {SelectedActivite.ID} à la date {date:dd/MM/yyyy} : {string.Join(", ", HorairesDisponibles)}");
            }

            HoraireSelectionne = null; 
        }




        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
           
                Frame.Navigate(typeof(PageType));
            
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
                    return;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(HoraireSelectionne))
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

                var selectedDate = DateTime.ParseExact((string)DatesDisponiblesComboBox.SelectedItem, "dd/MM/yyyy", null);
                var seanceCorrespondante = SingletonBDD.GetInstance()
                    .GetSeances(SelectedActivite.ID, selectedDate)
                    .FirstOrDefault(s => s.Horaire.ToString(@"hh\:mm") == HoraireSelectionne);

                if (seanceCorrespondante != null)
                {
                    string adherentID = utilisateurConnecte["ID"].ToString();
                    decimal? note = null;

                    var dialog = new ContentDialog
                    {
                        Title = "Avez-vous déjà participé à cette activité ?",
                        PrimaryButtonText = "Oui",
                        SecondaryButtonText = "Non",
                        CloseButtonText = "Annuler",
                        XamlRoot = this.XamlRoot
                    };

                    var result = await dialog.ShowAsync();
                    if (SingletonBDD.GetInstance().ParticipationExiste(adherentID, SelectedActivite.ID))
                    {
                        await new ContentDialog
                        {
                            Title = "Erreur",
                            Content = "Vous avez déjà réservé cette activité.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                        return;
                    }
                    if (SingletonBDD.GetInstance().ParticipationExiste(adherentID, SelectedActivite.ID))
                    {
                        await new ContentDialog
                        {
                            Title = "Erreur",
                            Content = "Vous avez déjà réservé cette activité.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                        return;
                    }

                    if (result == ContentDialogResult.Primary)
                    {
                        note = await DemanderNoteAsync();
                        if (note == null) return;
                    }

                    SingletonBDD.GetInstance().ReserverPlace(seanceCorrespondante.ID);
                    SingletonBDD.GetInstance().AjouterParticipation(adherentID, seanceCorrespondante.ID, note);

                    await new ContentDialog
                    {
                        Title = "Réservation réussie",
                        Content = "Votre réservation a été enregistrée avec succès.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();

                    Frame.Navigate(typeof(PageType));
                }
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


        private async Task<decimal?> DemanderNoteAsync()
        {
            StackPanel contentPanel = new StackPanel { Spacing = 10 };

            TextBlock textBlock = new TextBlock
            {
                Text = "Veuillez noter l'activité sur 5 étoiles :",
                FontSize = 16
            };

            RatingControl ratingControl = new RatingControl
            {
                MaxRating = 5,
                PlaceholderValue = 0
            };

            contentPanel.Children.Add(textBlock);
            contentPanel.Children.Add(ratingControl);

            ContentDialog dialog = new ContentDialog
            {
                Title = "Note de l'activité",
                Content = contentPanel,
                PrimaryButtonText = "Valider",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && ratingControl.Value > 0)
            {
                return (decimal)ratingControl.Value;
            }
            return null;
        }




        private async Task DemanderNoteAsync(string adherentID, int seanceID)
        {
            StackPanel contentPanel = new StackPanel { Spacing = 10 };

            TextBlock textBlock = new TextBlock
            {
                Text = "Veuillez noter l'activité sur 5 étoiles :",
                FontSize = 16
            };

            RatingControl ratingControl = new RatingControl
            {
                MaxRating = 5,
                PlaceholderValue = 0
            };

            contentPanel.Children.Add(textBlock);
            contentPanel.Children.Add(ratingControl);

            ContentDialog dialog = new ContentDialog
            {
                Title = "Note de l'activité",
                Content = contentPanel,
                PrimaryButtonText = "Valider",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                decimal note = (decimal)ratingControl.Value;

                try
                {
                    SingletonBDD.GetInstance().AjouterParticipation(adherentID, seanceID, note);

                    await new ContentDialog
                    {
                        Title = "Merci !",
                        Content = "Votre note a été enregistrée avec succès.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                }
                catch (Exception ex)
                {
                    await new ContentDialog
                    {
                        Title = "Erreur",
                        Content = $"Une erreur est survenue lors de l'enregistrement de la note : {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                }
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

                if (!SingletonBDD.GetInstance().EmailExiste(email) ||
                    !SingletonBDD.GetInstance().VerifierConnexion(email, password))
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





