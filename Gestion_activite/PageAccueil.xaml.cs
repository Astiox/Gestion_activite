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
            UpdateButtonStates();

        }

        private void ChargerActivitesDepuisBDD(int typeActiviteID)
        {
            try
            {
                SingletonBDD.TypeActiviteID = typeActiviteID; 
                var activitesBDD = SingletonBDD.GetInstance().GetActivitesParType(typeActiviteID);
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

            if (utilisateurConnecte != null)
            {
                ConnexionStatusTextBlock.Text = utilisateurConnecte["Role"].ToString() == "Admin"
                    ? utilisateurConnecte["Email"].ToString()
                    : $"Matricule: {utilisateurConnecte["ID"].ToString()}";
                ConnexionStatusTextBlock.Visibility = Visibility.Visible;

                ConnexionButton.Visibility = Visibility.Collapsed;
                InscriptionButton.Visibility = Visibility.Collapsed;
                DeconnexionButton.Visibility = Visibility.Visible;
            }
            else
            {
                ConnexionStatusTextBlock.Visibility = Visibility.Collapsed;
                ConnexionButton.Visibility = Visibility.Visible;
                InscriptionButton.Visibility = Visibility.Visible;
                DeconnexionButton.Visibility = Visibility.Collapsed;
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

            loginDialog.PrimaryButtonClick += (dlgSender, dlgArgs) =>
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

                var utilisateur = SingletonBDD.GetInstance().AuthentifierUtilisateur(email, password);

                if (utilisateur != null)
                {
                    SingletonBDD.SetUtilisateurConnecte(utilisateur);
                    Console.WriteLine($"Utilisateur connecté : {utilisateur["Nom"]} ({utilisateur["Role"]})");
                    UpdateButtonStates();
                }
                else
                {
                    errorMessage.Text = "Mot de passe ou adresse email incorrect.";
                    errorMessage.Visibility = Visibility.Visible;
                    dlgArgs.Cancel = true;
                }
            };

            await loginDialog.ShowAsync();
        }

        private void InscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageInscription));
        }

        private void DeconnexionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SingletonBDD.Deconnecter();

                UpdateButtonStates();

                Console.WriteLine("Déconnexion réussie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la déconnexion : {ex.Message}");
            }
        }




        private void BoutonAjoutActivite_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutActivite));
        }

        private void BoutonListeAdherents_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageListeAdherents));
        }
 
        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TypeActivite typeActivite)
            {
                TitleTextBlock.Text = $"Liste des activités - {typeActivite.Nom}";
                ChargerActivitesDepuisBDD(typeActivite.ID);
            }
            else if (e.Parameter is bool shouldReload && shouldReload)
            {
                var utilisateurConnecte = SingletonBDD.GetUtilisateurConnecte();
                if (utilisateurConnecte != null && utilisateurConnecte.ContainsKey("TypeActiviteID"))
                {
                    ChargerActivitesDepuisBDD((int)utilisateurConnecte["TypeActiviteID"]);
                }
            }
            else
            {
                TitleTextBlock.Text = "Liste des activités";
            }

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

        private void ActiviteCard_Click(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Activite activite)
            {
                Frame.Navigate(typeof(PageDetails), activite);
            }
        }
    }

}

