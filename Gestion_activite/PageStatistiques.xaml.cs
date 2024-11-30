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
    public sealed partial class PageStatistiques : Page
    {
        public int TotalAdherents { get; set; }
        public int TotalActivites { get; set; }
        public ObservableCollection<KeyValuePair<string, int>> AdherentsParActivite { get; set; }
        public ObservableCollection<KeyValuePair<string, decimal>> MoyennesNotesParActivite { get; set; }

        public PageStatistiques()
        {
            this.InitializeComponent();
            AdherentsParActivite = new ObservableCollection<KeyValuePair<string, int>>();
            MoyennesNotesParActivite = new ObservableCollection<KeyValuePair<string, decimal>>();

            ChargerStatistiques();
            this.DataContext = this;
        }
     
        private void ChargerStatistiques()
        {
            var singleton = SingletonBDD.GetInstance();

            TotalAdherents = singleton.GetTotalAdherents();
            TotalActivites = singleton.GetTotalActivites();

            AdherentsParActivite.Clear();
            foreach (var item in singleton.GetAdherentsParActivite())
            {
                AdherentsParActivite.Add(item);
            }

            MoyennesNotesParActivite.Clear();
            foreach (var item in singleton.GetMoyenneNotesParActivite())
            {
                MoyennesNotesParActivite.Add(item);
            }
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageType));
        }
    }
}
