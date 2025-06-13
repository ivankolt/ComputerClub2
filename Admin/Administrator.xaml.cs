using ComputerClub.Users;
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

namespace ComputerClub.Admin
{
    /// <summary>
    /// Логика взаимодействия для Administrator.xaml
    /// </summary>
    public partial class Administrator : Window
    {
        public Administrator()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadMapView();
        }
        private void LoadMapView()
        {
            var mapView = new MapView();

            MainContent.Content = mapView;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var users = new Users();
            MainContent.Content = users;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
        
            var shope = new Shope();
            MainContent.Content = shope; 
        }

        private void MenuItem_Click_Dash(object sender, RoutedEventArgs e)
        {
            var dashboardControl = new DashboardControl();
         
            MainContent.Content = dashboardControl; 
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            var shiftControl = new ShiftControl(CurrentUser.Instance.EmployeeId);
            MainContent.Content = shiftControl;
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            var ordersManagementControl = new OrdersManagementControl();
            MainContent.Content = ordersManagementControl;
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
    
}
