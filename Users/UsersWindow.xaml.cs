using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComputerClub.Users
{
    /// <summary>

    /// </summary>
    public partial class UsersWindow : Window
    {
        public UsersWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadMapView();
        }
        private void LoadMapView()
        {
            var mapView = new MapViewUsers();

            MainContent.Content = mapView;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
