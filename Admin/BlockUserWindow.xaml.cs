using ComputerClub.BD;
using System.Data;
using System.Windows;

namespace ComputerClub.Admin
{
    public partial class BlockUserWindow : Window
    {
        private DatabaseManager _dbManager = new DatabaseManager();
        private DataRow _selectedUser;

        public BlockUserWindow()
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
            userInfoBlock.Text = $"Логин: {_selectedUser["username"]}";
        }

        private void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Сначала найдите пользователя!");
                return;
            }

            if (string.IsNullOrWhiteSpace(reasonBox.Text))
            {
                MessageBox.Show("Укажите причину блокировки!");
                return;
            }

            if (_dbManager.BlockUser(
                (int)_selectedUser["id"],
                reasonBox.Text,
                (string)_selectedUser["card_number"]))
            {
                MessageBox.Show("Пользователь успешно заблокирован!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при блокировке пользователя!");
            }
        }
    }
}