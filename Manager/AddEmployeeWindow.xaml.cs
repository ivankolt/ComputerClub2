using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ComputerClub.BD;
using ComputerClub.Registration;

namespace ComputerClub.Manager
{
    public partial class AddEmployeeWindow : Window
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        private readonly Mail _mail = new Mail();
        private List<Position> _positions = new List<Position>();

        public AddEmployeeWindow()
        {
            InitializeComponent();
            LoadPositions();
            dpHireDate.SelectedDate = DateTime.Today;
        }

        private void LoadPositions()
        {
            try
            {
                _positions = _db.GetAllPositions();
                cmbPosition.ItemsSource = _positions;
                cmbPosition.DisplayMemberPath = "PositionName";
                cmbPosition.SelectedValuePath = "Id";
                
                if (_positions.Count > 0)
                {
                    cmbPosition.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке должностей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                var newEmployee = new NewEmployeeData
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    PhoneNumber = txtPhoneNumber.Text.Trim(),
                    Gender = rbMale.IsChecked == true ? "М" : "Ж",
                    PassportSeries = txtPassportSeries.Text.Trim(),
                    PassportNumber = txtPassportNumber.Text.Trim(),
                    PositionId = (int)cmbPosition.SelectedValue,
                    HireDate = dpHireDate.SelectedDate.Value,
                    Username = txtUsername.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Password = txtPassword.Password,
                    CardNumber = txtCardNumber.Text.Trim()
                };

                bool success = _db.AddNewEmployee(newEmployee);
                
                if (success)
                {
                    MessageBox.Show("Сотрудник успешно принят на работу", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
   
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ||
                string.IsNullOrWhiteSpace(txtPassportSeries.Text) ||
                string.IsNullOrWhiteSpace(txtPassportNumber.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtCardNumber.Text) ||
                cmbPosition.SelectedItem == null ||
                dpHireDate.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtPhoneNumber.Text, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Номер телефона должен быть в формате +7XXXXXXXXXX", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

        
            if (!Regex.IsMatch(txtPassportSeries.Text, @"^\d{4}$"))
            {
                MessageBox.Show("Серия паспорта должна состоять из 4 цифр", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtPassportNumber.Text, @"^\d{6}$"))
            {
                MessageBox.Show("Номер паспорта должен состоять из 6 цифр", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите корректный email", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }


            if (!Regex.IsMatch(txtCardNumber.Text, @"^\d{16}$"))
            {
                MessageBox.Show("Номер карты должен состоять из 16 цифр", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GenerateCardNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cardNumber;
                bool isUnique = false;

                do
                {
                    // Generate a 16-digit number
                    cardNumber = GenerateNumericString(16);
                    
                    // Check if it's unique in the database
                    isUnique = _db.IsCardNumberUnique(cardNumber);
                    
                } while (!isUnique);

                // Set the generated card number to the text box
                txtCardNumber.Text = cardNumber;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации номера карты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateNumericString(int length)
        {
            // Use only digits for card number
            const string digits = "0123456789";
            var random = new Random();
            
            // Make sure the first digit is not 0
            char firstDigit = digits[random.Next(1, digits.Length)];
            
            // Generate the rest of the digits
            string restOfNumber = new string(Enumerable.Repeat(digits, length - 1)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return firstDigit + restOfNumber;
        }
    }

    public class Position
    {
        public int Id { get; set; }
        public string PositionName { get; set; }
        public decimal Salary { get; set; }
    }

    public class NewEmployeeData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public int PositionId { get; set; }
        public DateTime HireDate { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CardNumber { get; set; }
    }
}
