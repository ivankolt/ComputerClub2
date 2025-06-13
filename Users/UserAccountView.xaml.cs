using ComputerClub.BD;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using ComputerClub.Entrance;

namespace ComputerClub.Users
{
    public partial class UserAccountView : UserControl, INotifyPropertyChanged
    {
        private DatabaseManager _dbManager = new DatabaseManager();
        private int _currentUserId;

        private UserFullInfo _userInfo;
        private IEnumerable<Payment> _userPayments;

        public event PropertyChangedEventHandler PropertyChanged;

        public UserAccountView()
        {
            InitializeComponent();
            _currentUserId = CurrentUser.Instance.Id;
            DataContext = this;
            LoadUserData();
            LoadReceipts();
        }

        private void LoadUserData()
        {
            UserInfo = _dbManager.GetFullUserInfo(_currentUserId);
        }

        private void LoadReceipts()
        {
            UserPayments = _dbManager.GetUserPayments(_currentUserId);
        }

        public UserFullInfo UserInfo
        {
            get => _userInfo;
            set
            {
                _userInfo = value;
                OnPropertyChanged(nameof(UserInfo));
            }
        }

        public IEnumerable<Payment> UserPayments
        {
            get => _userPayments;
            set
            {
                _userPayments = value;
                OnPropertyChanged(nameof(UserPayments));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Введите новый пароль!");
                return;
            }

            if (_dbManager.UpdatePassword(_currentUserId, newPassword))
            {
                MessageBox.Show("Пароль успешно изменён!");
                NewPasswordBox.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка при изменении пароля!");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _dbManager.IsNotActive(CurrentUser.Instance.Id);
            _dbManager.InsertUserAction("Выход", CurrentUser.Instance.Id);
            CurrentUser.Instance = null;

            var window = Window.GetWindow(this);
            if (window != null)
            {
                var loginWindow = new Input();
                loginWindow.Show();

                window.Close();
            }
        }
    }
}