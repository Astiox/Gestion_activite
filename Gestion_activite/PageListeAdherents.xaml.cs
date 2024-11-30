using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace Gestion_activite
{
    public sealed partial class PageListeAdherents : Page
    {
        public ObservableCollection<Adherent> Adherents { get; set; } = new ObservableCollection<Adherent>();

        public PageListeAdherents()
        {
            this.InitializeComponent();
            LoadAdherents();
        }

        private void LoadAdherents()
        {
            Adherents.Clear();
            var adherentsBDD = SingletonBDD.GetInstance().ObtenirAdherents();
            foreach (var adherent in adherentsBDD)
            {
                Adherents.Add(new Adherent
                {
                    ID = adherent["ID"].ToString(),
                    Nom = adherent["Nom"].ToString(),
                    Prenom = adherent["Prenom"].ToString()
                });
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is bool shouldReload && shouldReload)
            {
                LoadAdherents(); 
            }
        }
        private void SupprimerAdherent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Adherent adherent)
            {
                SingletonBDD.GetInstance().SupprimerAdherent(adherent.ID);
                Adherents.Remove(adherent);
            }
        }


        private void ModifierAdherent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Adherent adherent)
            {
                Frame.Navigate(typeof(PageModificationAdherent), adherent);
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

    }
}
