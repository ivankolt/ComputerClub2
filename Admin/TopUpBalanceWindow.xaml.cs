using ComputerClub.BD;
using System;
using System.Data;
using System.Windows;

namespace ComputerClub.Admin
{
    public partial class TopUpBalanceWindow : Window
    {
        private DatabaseManager _dbManager = new DatabaseManager();
        private DataRow _selectedUser;

        public TopUpBalanceWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string cardNumber = cardNumberBox.Text.Trim();
            if (cardNumber.Length != 16)
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр!");
                return;
            }

            DataTable result = _dbManager.GetUserByCardNumber(cardNumber);
            if (result.Rows.Count == 0)
            {
                MessageBox.Show("Пользователь не найден!");
                return;
            }

            _selectedUser = result.Rows[0];
            userInfoBlock.Text = $"Логин: {_selectedUser["username"]}\nТекущий баланс: {_selectedUser["balance"]} ₽";
        }

        private void TopUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Сначала найдите пользователя!");
                return;
            }

            if (!decimal.TryParse(amountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму!");
                return;
            }

            if (_dbManager.UpdateUserBalance(
                (int)_selectedUser["id"],
                amount,
                (string)_selectedUser["card_number"]))
            {
                MessageBox.Show("Счет успешно пополнен!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при пополнении счета!");
            }
        }
    }
}