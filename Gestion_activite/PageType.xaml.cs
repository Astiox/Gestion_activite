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
    public sealed partial class PageType : Page
    {
        public ObservableCollection<TypeActivite> TypesActivites { get; set; } = new ObservableCollection<TypeActivite>();

        public PageType()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadTypesActivites();
            UpdateButtonStates();
        }

        private void LoadTypesActivites()
        {
            var types = SingletonBDD.GetInstance().GetTypesActivites();

            TypesActivites.Clear();
            foreach (var type in types)
            {
                TypesActivites.Add(type);
            }
        }

        private void TypeCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TypeActivite typeActivite)
            {
                Frame.Navigate(typeof(PageAccueil), typeActivite);
            }
        }

        private void UpdateButtonStates()
        {
            var utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
            bool isLoggedIn = utilisateurConnecte != null;

            ConnexionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            InscriptionButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            DeconnexionButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            if (isLoggedIn)
            {
                string role = utilisateurConnecte["Role"].ToString();
                string infoTexte = role == "Admin"
                    ? $"Admin : {utilisateurConnecte["Email"]}"
                    : $"Adhérent : {utilisateurConnecte["Nom"]}";

                UserInfoTextBlock.Text = infoTexte;
                UserInfoTextBlock.Visibility = Visibility.Visible;

                if (role == "Admin")
                {
                    ListeAdherentsButton.Visibility = Visibility.Visible;
                    StatistiquesButton.Visibility = Visibility.Visible;
                    AjoutActiviteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ListeAdherentsButton.Visibility = Visibility.Collapsed;
                    StatistiquesButton.Visibility = Visibility.Collapsed;
                    AjoutActiviteButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                UserInfoTextBlock.Visibility = Visibility.Collapsed;

                ListeAdherentsButton.Visibility = Visibility.Collapsed;
                StatistiquesButton.Visibility = Visibility.Collapsed;
                AjoutActiviteButton.Visibility = Visibility.Collapsed;
            }
        }


        private async void ConnexionButton_Click(object sender, RoutedEventArgs e)
        {
            await DemanderConnexionAsync();
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
                PrimaryButtonText = "Se connecter",
                CloseButtonText = "Annuler",
                XamlRoot = this.XamlRoot
            };

            loginDialog.PrimaryButtonClick += (dlgSender, dlgArgs) =>
            {
                string email = emailInput.Text.Trim();
                string password = passwordInput.Password.Trim();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    errorMessage.Text = "Veuillez remplir tous les champs.";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                    return;
                }

                var utilisateur = SingletonBDD.GetInstance().AuthentifierUtilisateur(email, password);

                if (utilisateur != null)
                {
                    SingletonBDD.SetUtilisateurConnecte(utilisateur);
                    UpdateButtonStates();
                }
                else
                {
                    errorMessage.Text = "Adresse email ou mot de passe incorrect.";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                }
            };

            await loginDialog.ShowAsync();
        }


        private void Logo_Click(object sender, PointerRoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var hoverOverlay = grid.FindName("HoverOverlay") as Border;
                if (hoverOverlay != null)
                {
                    hoverOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var hoverOverlay = grid.FindName("HoverOverlay") as Border;
                if (hoverOverlay != null)
                {
                    hoverOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void SupprimerTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TypeActivite type)
            {
                SingletonBDD.GetInstance().SupprimerTypeActivite(type.ID);
                LoadTypesActivites();
            }
        }

        private void ListeAdherentsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageListeAdherents));
        }

        private void StatistiquesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageStatistiques));
        }

        private void AjoutActiviteButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutActivite));
        }
    }
}