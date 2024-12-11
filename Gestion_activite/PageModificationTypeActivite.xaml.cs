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
    public sealed partial class PageModificationTypeActivite : Page
    {
        public ObservableCollection<TypeActivite> Categories { get; set; }

        public PageModificationTypeActivite()
        {
            this.InitializeComponent();
            Categories = new ObservableCollection<TypeActivite>();
            LoadCategories();
        }

        // Charger les catégories
        private void LoadCategories()
        {
            try
            {
                var categoriesBDD = SingletonBDD.GetInstance().GetTypesActivites();
                foreach (var categorie in categoriesBDD)
                {
                    Categories.Add(new TypeActivite
                    {
                        ID = Convert.ToInt32(categorie.ID),
                        Nom = categorie.Nom,
                        Description = categorie.Description,
                        Image = categorie.Image
                    });
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

        // Lorsque l'utilisateur sélectionne une catégorie dans la ComboBox
        private void CategorieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorieComboBox.SelectedItem is TypeActivite selectedCategorie)
            {
                nomcat.Text = selectedCategorie.Nom;
                desccat.Text = selectedCategorie.Description;
                image.Text = selectedCategorie.Image;
            }
        }

        // Retour à la page principale
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }

        // Validation et modification de la catégorie
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CategorieComboBox.SelectedItem is TypeActivite selectedCategorie)
            {
                string nom = nomcat.Text;
                string description = desccat.Text;
                string imageUrl = image.Text;

                if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl))
                {
                    ShowErrorMessage("Veuillez remplir tous les champs.");
                    return;
                }

                try
                {
                    SingletonBDD.GetInstance().ModifierCategorie(
                        selectedCategorie.ID,
                        nom,
                        description,
                        imageUrl);

                    ShowSuccessMessage("Catégorie modifiée avec succès !");
                    Frame.Navigate(typeof(PageType));
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Erreur lors de la modification : {ex.Message}");
                }
            }
        }

        private async void ShowErrorMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void ShowSuccessMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Succès",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }
    }
}
