using ComputerClub.BD;
using Microsoft.Win32;
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

namespace ComputerClub.Registration
{
    /// <summary>
    /// Логика взаимодействия для Reg.xaml
    /// </summary>
    public partial class Reg : Window
    {
        public Reg()
        {
            InitializeComponent();
        }
        private Mail mail = new Mail();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = Login.Text.Trim();
            string password = Password.Text.Trim();
            string confirmPassword = Password2.Text.Trim();
            string email = Mail.Text.Trim();
            string firstName = FirstName.Text.Trim();
            string lastName = LastName.Text.Trim();
            string phoneNumber = PhoneNumber.Text.Trim();

            try
            {
                phoneNumber = ValidatePhoneNumber(phoneNumber);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

       
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(phoneNumber))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatabaseManager dbManager = new DatabaseManager();

            string generatedPassword = mail.GenerateRandomPassword();
            mail.SendMessage(email, generatedPassword);

           
            Registr registrWindow = new Registr(generatedPassword, username, password, email, firstName, lastName, phoneNumber);
            registrWindow.ShowDialog();

            if (registrWindow.IsPasswordConfirmed)
            {
                bool isRegistered = dbManager.RegisterUser(username, password, email, firstName, lastName, phoneNumber);

                if (isRegistered)
                {
                    MessageBox.Show("Регистрация успешно завершена!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
        }

        private string ValidatePhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (phoneNumber.StartsWith("+7") && phoneNumber.Length == 12 && long.TryParse(phoneNumber.Substring(2), out _))
            {
                return phoneNumber;
            }

            throw new ArgumentException("Некорректный формат номера телефона. Используйте формат +7XXXXXXXXXX.");
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
        private void Login_GotFocus1(object sender, RoutedEventArgs e)
        {
            if (Mail.Text == "Почта")
            {
                Mail.Text = string.Empty;
            }
        }

        private void Login_LostFocus1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Mail.Text))
            {
                Mail.Text = "Почта";
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
 
        private void Login_GotFocus3(object sender, RoutedEventArgs e)
        {
            if (Password2.Text == "Повторите пароль")
            {
                Password2.Text = string.Empty;
            }
        }

        private void Login_LostFocus3(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Password2.Text))
            {
                Password2.Text = "Повторите пароль";
            }
        }
        private void FirstName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FirstName.Text == "Имя")
            {
                FirstName.Text = string.Empty;
            }
        }

        private void FirstName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FirstName.Text))
            {
                FirstName.Text = "Имя";
            }
        }

        private void LastName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LastName.Text == "Фамилия")
            {
                LastName.Text = string.Empty;
            }
        }

        private void LastName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LastName.Text))
            {
                LastName.Text = "Фамилия";
            }
        }

        private void PhoneNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneNumber.Text == "Номер телефона")
            {
                PhoneNumber.Text = string.Empty;
            }
        }

        private void PhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PhoneNumber.Text))
            {
                PhoneNumber.Text = "Номер телефона";
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Entrance.Input input = new Entrance.Input();

            input.Show();
            this.Close();
        }
    }
}
