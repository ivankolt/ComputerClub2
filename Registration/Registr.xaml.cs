using ComputerClub.Entrance;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerClub.Registration
{
    public partial class Registr : Window
    {
        public static string GeneratedPassword { get; private set; }
        public bool IsPasswordConfirmed { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PhoneNumber { get; private set; }

        public Registr(string generatedPassword, string username, string password, string email, string firstName, string lastName, string phoneNumber)
        {
            InitializeComponent();
            GeneratedPassword = generatedPassword;
            Username = username;
            Password = password;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PasswordMail.Text) && PasswordMail.Text == GeneratedPassword)
            {
                IsPasswordConfirmed = true;
                var input = new Input();
                input.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Border_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordMail.Text == "Код")
            {
                PasswordMail.Text = string.Empty;
            }
        }

        private void Border_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordMail.Text))
            {
                PasswordMail.Text = "Код";
            }
        }
    }
}