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
using System.ComponentModel;
using System.Diagnostics;
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
    public sealed partial class PageType : Page, INotifyPropertyChanged
    {
        public ObservableCollection<TypeActivite> TypesActivites { get; set; } = new ObservableCollection<TypeActivite>();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsAdmin
        {
            get
            {
                var utilisateur = SingletonBDD.GetUtilisateurConnecte();
                return utilisateur != null && utilisateur["Role"].ToString() == "Admin";
            }
        }
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

            ListeAdherentsButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            StatistiquesButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            AjoutActiviteButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            ExporterButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            AjoutAdherentButton.Visibility = IsAdmin ? Visibility.Visible: Visibility.Collapsed;
            AjoutAdministrateurButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            AjoutCategorieButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            AjoutSeanceButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            ModificationActiviteButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            ModificationCategorieButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            ModificationSeanceButton.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;



            if (isLoggedIn)
            {
                string userInfo = IsAdmin
                    ? $"Admin : {utilisateurConnecte["Email"]}"
                    : $"Adhérent : {utilisateurConnecte["ID"]}";

                UserInfoTextBlock.Text = userInfo;
                UserInfoTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                UserInfoTextBlock.Visibility = Visibility.Collapsed;
            }

            NotifyPropertyChanged(nameof(IsAdmin));
        }

        private void IctrType_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonStates();
        }

        private async void ConnexionButton_Click(object sender, RoutedEventArgs e)
        {
            await DemanderConnexionAsync();
            LoadTypesActivites();
        }

        private void InscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageInscription));
        }

        private void DeconnexionButton_Click(object sender, RoutedEventArgs e)
        {
            SingletonBDD.Deconnecter();
            UpdateButtonStates();
            LoadTypesActivites();
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

        private void ModifierTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TypeActivite type)
            {
                Frame.Navigate(typeof(PageModificationTypeActivite), type);
            }
        }

        private async void SupprimerTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TypeActivite type)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirmation de suppression",
                    Content = $"Êtes-vous sûr de vouloir supprimer le type '{type.Nom}' ?",
                    PrimaryButtonText = "Oui",
                    CloseButtonText = "Annuler",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    SingletonBDD.GetInstance().SupprimerTypeActivite(type.ID);
                    TypesActivites.Remove(type);
                }
            }
        }

        
        private async Task ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Information",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void ExportToCsv(string fileName, List<Dictionary<string, object>> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                {
                    _ = ShowMessage("Aucune donnée à exporter.");
                    return;
                }

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    var headers = data.First().Keys;
                    writer.WriteLine(string.Join(",", headers));

                    foreach (var row in data)
                    {
                        var values = headers.Select(h => row.ContainsKey(h) ? row[h]?.ToString() : string.Empty);
                        writer.WriteLine(string.Join(",", values));
                    }
                }

                _ = ShowMessage($"Fichier exporté avec succès : {filePath}");
            }
            catch (Exception ex)
            {
                _ = ShowMessage($"Erreur lors de l'exportation : {ex.Message}");
            }
        }

        

        private void ExporterTout()
        {
            try
            {
                var adherents = SingletonBDD.GetInstance().ObtenirAdherents();
                var activites = SingletonBDD.GetInstance().ObtenirActivites();

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdherentActivite.csv");

                using (var writer = new StreamWriter(filePath))
                {
                    if (adherents != null && adherents.Any())
                    {
                        writer.WriteLine("Liste des Adhérents");
                        var headers = adherents.First().Keys;
                        writer.WriteLine(string.Join(",", headers));
                        foreach (var row in adherents)
                        {
                            var values = headers.Select(h => row[h]?.ToString() ?? string.Empty);
                            writer.WriteLine(string.Join(",", values));
                        }
                    }

                    if (activites != null && activites.Any())
                    {
                        writer.WriteLine("\nListe des Activités");
                        var headers = activites.First().Keys;
                        writer.WriteLine(string.Join(",", headers));
                        foreach (var row in activites)
                        {
                            var values = headers.Select(h => row[h]?.ToString() ?? string.Empty);
                            writer.WriteLine(string.Join(",", values));
                        }
                    }
                }

                _ = ShowMessage($"Fichier exporté avec succès : {filePath}");
            }
            catch (Exception ex)
            {
                _ = ShowMessage($"Erreur lors de l'exportation complète : {ex.Message}");
            }
        }
        private async void ExporterButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Exporter les données",
                Content = "Choisissez les données à exporter.",
                PrimaryButtonText = "Adhérents",
                SecondaryButtonText = "Activités",
                CloseButtonText = "Tout exporter",
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonCommandParameter = "Annuler",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    ExportAdherents();
                    break;
                case ContentDialogResult.Secondary:
                    ExportActivites();
                    break;
                case ContentDialogResult.None:
                    ExporterTout();
                    break;
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
        private void ExportAdherents()
        {
          var adherents = SingletonBDD.GetInstance().ObtenirAdherents();
          ExportToCsv("Adherents.csv", adherents);
        }

        private void ExportActivites()
        {
          var activites = SingletonBDD.GetInstance().ObtenirActivites();
          ExportToCsv("Activites.csv", activites);
        }
        private void AjoutAdherentButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutAdherent));
        }

        private void AjoutSeanceButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutSeance));
        }

        private void AjoutCategorieButton_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutTypeActivite));
        }

        private void AjoutAdministrateurButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAjoutAdministrateur));
        }

        private void ModificationCategorieButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageModificationTypeActivite));

        }

        private void ModificationActiviteButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageModificationActivite));

        }

        private void ModificationSeanceButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageModificationTypeActivite));

        }
    }
}