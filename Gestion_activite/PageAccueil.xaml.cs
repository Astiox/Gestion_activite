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

        public bool IsLoggedIn = false;

        public string CurrentUser = null;
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


                    Console.WriteLine($"Nombre d'activités chargées : {Activites.Count}");
                }
                else
                {
                    Console.WriteLine("Aucune activité récupérée depuis la base de données.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités : {ex.Message}");
            }
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

        private void ConnexionButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as Button).Background = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        }

        private void ConnexionButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as Button).Background = new SolidColorBrush(Microsoft.UI.Colors.LightGreen);
        }

        private async void ConnexionButton_Click(object sender, RoutedEventArgs e)
        {
            var emailInput = new TextBox { PlaceholderText = "Email", Name = "EmailInput" };
            var passwordInput = new PasswordBox { PlaceholderText = "Mot de passe", Name = "PasswordInput" };
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

            ContentDialog loginDialog = new ContentDialog
            {
                Title = "Connexion",
                Content = loginContent,
                PrimaryButtonText = "Connexion",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            loginDialog.PrimaryButtonClick += async (dlgSender, dlgArgs) =>
            {
                string email = emailInput.Text;
                string password = passwordInput.Password;

                if (!SingletonBDD.GetInstance().EmailExiste(email))
                {
                    errorMessage.Text = "Mot de passe ou Adresse email incorrect";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                    return;
                }

                if (!SingletonBDD.GetInstance().VerifierConnexion(email, password))
                {
                    errorMessage.Text = "Mot de passe ou Adresse email incorrect";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                    return;
                }

                App.Current.Resources["IsLoggedIn"] = true;
                App.Current.Resources["CurrentUser"] = email;
                UpdateButtonStates();
            };

            _ = await loginDialog.ShowAsync();
        }



        private void UpdateButtonStates()
        {
            bool isLoggedIn = (bool)App.Current.Resources["IsLoggedIn"];
            ConnexionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            InscriptionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            DeconnexionButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            ConnexionStatusCircle.Fill = isLoggedIn
                ? new SolidColorBrush(Microsoft.UI.Colors.Green)
                : new SolidColorBrush(Microsoft.UI.Colors.Red);
        }


        private void DeconnexionButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Resources["IsLoggedIn"] = false;
            App.Current.Resources["CurrentUser"] = "None";
            UpdateButtonStates();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateButtonStates();
        }




        private void ActiviteCard_Click(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Activite activite)
            {
                Frame.Navigate(typeof(PageDetails), activite);
            }
        }





        private void InscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageInscription));
        }
    }
}
