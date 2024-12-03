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
    public sealed partial class PageAjoutActivite : Page
    {
        public ObservableCollection<TypeActivite> Categories { get; set; }

        public PageAjoutActivite()
        {
            this.InitializeComponent();
            Categories = new ObservableCollection<TypeActivite>();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                var types = SingletonBDD.GetInstance().GetTypesActivites();
                foreach (var type in types)
                {
                    Categories.Add(type);
                }

                CategorieComboBox.ItemsSource = Categories;
                CategorieComboBox.DisplayMemberPath = "Nom"; 
                CategorieComboBox.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des catégories : {ex.Message}");
            }
        }

        private void AjouterActiviteButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = NomActiviteInput.Text;
            string description = DescriptionInput.Text;
            string imageUrl = ImageInput.Text;
            decimal coutOrganisation;
            decimal prixVente;
            int typeActiviteID;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(imageUrl) || !decimal.TryParse(PrixOrgInput.Text, out coutOrganisation) ||
                !decimal.TryParse(PrixVenteInput.Text, out prixVente) || CategorieComboBox.SelectedItem == null)
            {
                ShowErrorMessage("Veuillez remplir tous les champs correctement.");
                return;
            }

            typeActiviteID = ((TypeActivite)CategorieComboBox.SelectedItem).ID;

            if (SingletonBDD.GetInstance().ActiviteExiste(nom))
            {
                ShowErrorMessage("Une activité avec ce nom existe déjà.");
                return;
            }

            try
            {
                SingletonBDD.GetInstance().AjouterActivite(nom, typeActiviteID, description, coutOrganisation, prixVente, imageUrl);
                ShowSuccessMessage("Activité ajoutée avec succès !");
                Frame.Navigate(typeof(PageType));
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Une erreur est survenue : {ex.Message}");
            }
        }



        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        private async void ShowErrorMessage(string message)
        {
            await new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
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


    }
    

}