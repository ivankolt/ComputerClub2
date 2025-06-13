using ComputerClub.Admin;
using ComputerClub.BD;
using ComputerClub.Manager;
using ComputerClub.Registration;
using ComputerClub.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

namespace ComputerClub.Entrance
{
    public partial class Input : Window
    {
        DatabaseManager databaseManager = new DatabaseManager();
        public Input()
        {
            InitializeComponent();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Login_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Login.Text == "Логин")
            {
                Login.Text = string.Empty;
            }
        }

        private void Login_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Login.Text))
            {
                Login.Text = "Логин";
            }
        }

   
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = Login.Text.Trim();
            string password = Password.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string userRole = databaseManager.AuthenticateUser(username, password);

            if(userRole == null)
            {
                MessageBox.Show("Такого пользователя не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            switch (userRole.ToLower())
            {
                case "admin":
                    MessageBox.Show($"Добро пожаловать, Администратор {username}!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenAdminInterface();
                    break;

                case "users":
                    MessageBox.Show($"Добро пожаловать, Пользователь {username}!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenUserInterface();
                    break;

                case "manager":
                    MessageBox.Show($"Добро пожаловать, Менеджер {username}!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenManagerInterface();
                    break;

                default:
                    MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }
        private void OpenAdminInterface()
        {
            this.Hide();
            Administrator adminWindow = new Administrator();
            CurrentUser.Instance.EmployeeId = databaseManager.CurrentEmployee(CurrentUser.Instance.Id);
            adminWindow.Show();
        }
        private void OpenUserInterface()
        {
            bool Answer = false;
            string Reason = null;

            (Answer, Reason) = databaseManager.BlockedOrNot(CurrentUser.Instance.Id);
            databaseManager.InsertUserAction("Вход", CurrentUser.Instance.Id);
            if (Answer != true)
            {
                this.Hide();
                UsersWindow userWindow = new UsersWindow();
                databaseManager.IsActive(CurrentUser.Instance.Id);
                userWindow.Show();
            }
            else 
            {
                MessageBox.Show("Вас заблокировали по причине " + Reason + "! ");
            }

        }
        private void OpenManagerInterface()
        {
            this.Hide();
            var managerWindow = new ManagerWindow();
            managerWindow.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Reg registr = new Reg();
            registr.Show();
            this.Close();
        }
    }
}
