using ComputerClub.Admin;
using ComputerClub.BD;
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

        private void Login_GotFocus2(object sender, RoutedEventArgs e)
        {
            if (Password.Text == "Пароль")
            {
                Password.Text = string.Empty;
            }
        }

        private void Login_LostFocus2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Password.Text))
            {
                Password.Text = "Пароль";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = Login.Text.Trim();
            string password = Password.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DatabaseManager databaseManager = new DatabaseManager();

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

                case "user":
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
            adminWindow.Show();
        }
        private void OpenUserInterface()
        {
            this.Hide();
            UsersWindow userWindow = new UsersWindow();
            userWindow.Show();
        }
        private void OpenManagerInterface()
        {
            this.Hide();
            Administrator managerWindow = new Administrator();
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
