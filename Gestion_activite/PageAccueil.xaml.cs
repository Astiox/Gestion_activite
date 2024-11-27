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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageAccueil : Page
    {
        public ObservableCollection<Activite> Activites { get; set; }

        public PageAccueil()
        {
            this.InitializeComponent();
            Activites = new ObservableCollection<Activite>();
            ChargerActivitesDepuisBDD();
        }

        private void ChargerActivitesDepuisBDD()
        {
            try
            {
                var activitesBDD = SingletonBDD.GetInstance().GetActivites();
                if (activitesBDD != null && activitesBDD.Count > 0)
                {
                    Activites.Clear();
                    foreach (var activite in activitesBDD)
                    {
                        Activites.Add(activite);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités : {ex.Message}");
            }
        }

        private void UpdateButtonStates()
        {
            var utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
            bool isLoggedIn = utilisateurConnecte != null;
            bool isAdmin = isLoggedIn && utilisateurConnecte["Role"].ToString() == "Admin";

            ConnexionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            InscriptionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            DeconnexionButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            BoutonAjoutActivite.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BoutonListeAdherents.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            ConnexionStatusCircle.Fill = isLoggedIn ? new SolidColorBrush(Microsoft.UI.Colors.Green) : new SolidColorBrush(Microsoft.UI.Colors.Red);
        }

        private void DisplayUserInfo()
        {
            var utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
            if (utilisateurConnecte != null)
            {
                string role = utilisateurConnecte["Role"].ToString();
                string nom = utilisateurConnecte["Nom"].ToString();
                Console.WriteLine($"Connecté en tant que {role}: {nom}");
            }
            else
            {
                Console.WriteLine("Aucun utilisateur connecté.");
            }
        }


        private async void ConnexionButton_Click(object sender, RoutedEventArgs e)
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

            loginDialog.PrimaryButtonClick += async (dlgSender, dlgArgs) =>
            {
                try
                {
                    string email = emailInput.Text;
                    string password = passwordInput.Password;

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        errorMessage.Text = "Veuillez remplir tous les champs.";
                        errorMessage.Visibility = Visibility.Visible;
                        dlgArgs.Cancel = true;
                        return;
                    }

                    if (!SingletonBDD.GetInstance().EmailExiste(email))
                    {
                        errorMessage.Text = "Mot de passe ou adresse email incorrect.";
                        errorMessage.Visibility = Visibility.Visible;
                        dlgArgs.Cancel = true;
                        return;
                    }

                    if (!SingletonBDD.GetInstance().VerifierConnexion(email, password))
                    {
                        errorMessage.Text = "Mot de passe ou adresse email incorrect.";
                        errorMessage.Visibility = Visibility.Visible;
                        dlgArgs.Cancel = true;
                        return;
                    }

                    var utilisateur = SingletonBDD.GetInstance().AuthentifierUtilisateur(email, password);
                    SingletonBDD.SetUtilisateurConnecte(utilisateur);

                    UpdateButtonStates();

                    dlgArgs.Cancel = false;
                }
                catch (Exception ex)
                {
                    errorMessage.Text = $"Erreur : {ex.Message}";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                }
            };


            try
            {
                await loginDialog.ShowAsync();
            }
            catch (InvalidOperationException)
            {
                errorMessage.Text = "Une autre fenêtre de dialogue est déjà ouverte. Veuillez fermer l'autre dialogue.";
                errorMessage.Visibility = Visibility.Visible;
            }
        }




        private void InscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageInscription));
        }

        private void DeconnexionButton_Click(object sender, RoutedEventArgs e)
        {
            SingletonBDD.Deconnecter();
            UpdateButtonStates();
        }

        private void BoutonAjoutActivite_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutActivite));
        }

        private void BoutonListeAdherents_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageListeAdherents));
        }

        private void ActiviteCard_Click(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Activite activite)
            {
                Frame.Navigate(typeof(PageDetails), activite);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateButtonStates();
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var overlay = grid.FindName("HoverOverlay") as Border;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var overlay = grid.FindName("HoverOverlay") as Border;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Collapsed;
                }
            }
        }

    }
}

