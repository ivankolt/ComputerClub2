using ComputerClub.Admin;
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

using ComputerClub.Entrance;

namespace ComputerClub.Manager
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        public ManagerWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addPCControl = new AddPCControl();

            MainContent.Content = addPCControl;
        }

        private void Shop_Click(object sender, RoutedEventArgs e)
        {
            var employeesControl = new EmployeesControl();

            MainContent.Content = employeesControl;
            
        }

        private void ClubCardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var userControl = new UsersControl();

            MainContent.Content = userControl;
        }

        private void OrdersClients_Click(object sender, RoutedEventArgs e)
        {
            var userControl = new OrdersManagementControl();

            MainContent.Content = userControl;
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы точно хотите выйти? Да или Нет",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Input input = new Input();
                input.Show();
                this.Close();
            }
      
        }
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
