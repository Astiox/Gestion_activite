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
    public sealed partial class PageModificationActivite : Page
    {
        public ObservableCollection<Activite> Activites { get; set; }
        public ObservableCollection<TypeActivite> Categories { get; set; }

        public PageModificationActivite()
        {
            this.InitializeComponent();
            Activites = new ObservableCollection<Activite>();
            Categories = new ObservableCollection<TypeActivite>();
            LoadData();
        }
        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void LoadData()
        {
            try
            {
                LoadActivites();
                LoadCategories();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des données : {ex.Message}");
            }
        }

        private void LoadActivites()
        {
            try
            {
                var activitesBDD = SingletonBDD.GetInstance().ObtenirActivites();
                if (activitesBDD.Count == 0)
                {
                    ShowErrorMessage("Aucune activité trouvée.");
                    return;
                }

                Activites.Clear(); 

                foreach (var activite in activitesBDD)
                {
                    Activites.Add(new Activite
                    {
                        ID = int.Parse(activite["ID"].ToString()),
                        Nom = activite["Nom"].ToString(),
                        Description = activite["Description"].ToString()
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


        private void LoadCategories()
        {
            try
            {
                var categoriesBDD = SingletonBDD.GetInstance().GetTypesActivites();
                Categories.Clear();

                foreach (var categorie in categoriesBDD)
                {
                    Categories.Add(new TypeActivite
                    {
                        ID = categorie.ID,
                        Nom = categorie.Nom
                    });
                }

                catactiv.ItemsSource = Categories;
                catactiv.DisplayMemberPath = "Nom";
                catactiv.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des catégories : {ex.Message}");
            }
        }

        private void ActiviteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiviteComboBox.SelectedItem is Activite selectedActivite)
            {
                NomActiviteInput.Text = selectedActivite.Nom;
                DescriptionInput.Text = selectedActivite.Description;
                PrixOrgInput.Text = selectedActivite.CoutOrganisation.ToString();
                PrixVenteInput.Text = selectedActivite.PrixVente.ToString();
                ImageInput.Text = selectedActivite.Image;

                foreach (var item in Categories)
                {
                    if (item.ID == selectedActivite.TypeActiviteID)
                    {
                        catactiv.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private async void ModifierButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiviteComboBox.SelectedItem is Activite selectedActivite)
            {
                string nom = NomActiviteInput.Text;
                string description = DescriptionInput.Text;
                string image = ImageInput.Text;
                decimal coutOrganisation;
                decimal prixVente;
                int typeActiviteID;

                if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(description) ||
                    string.IsNullOrWhiteSpace(image) || !decimal.TryParse(PrixOrgInput.Text, out coutOrganisation) ||
                    !decimal.TryParse(PrixVenteInput.Text, out prixVente) || catactiv.SelectedItem == null)
                {
                    await ShowErrorMessage("Veuillez remplir tous les champs correctement.");
                    return;
                }

                typeActiviteID = ((TypeActivite)catactiv.SelectedItem).ID;

                try
                {
                    SingletonBDD.GetInstance().ModifierActivite(
                        selectedActivite.ID,
                        nom,
                        typeActiviteID,
                        description,
                        coutOrganisation,
                        prixVente,
                        image);

                    await ShowSuccessMessage("Activité modifiée avec succès !");
                    Frame.Navigate(typeof(PageType));
                }
                catch (Exception ex)
                {
                    await ShowErrorMessage($"Une erreur est survenue : {ex.Message}");
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private async void ShowDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            if (this.XamlRoot == null)
            {
                Console.WriteLine("Erreur : XamlRoot est null. Le ContentDialog ne peut pas être affiché.");
                return;
            }

            dialog.XamlRoot = this.XamlRoot;

            try
            {
                await dialog.ShowAsync();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Erreur lors de l'affichage du ContentDialog : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une autre erreur est survenue : {ex.Message}");
            }
        }

        private async Task ShowErrorMessage(string message)
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
                Console.WriteLine($"Erreur avec le ContentDialog : {ex.Message}");
            }
        }

        private async Task ShowSuccessMessage(string message)
        {
            if (this.XamlRoot == null)
            {
                Console.WriteLine("Erreur : XamlRoot est null. Le ContentDialog ne peut pas être affiché.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Succès",
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
                Console.WriteLine($"Erreur avec le ContentDialog : {ex.Message}");
            }
        }



    }
}
