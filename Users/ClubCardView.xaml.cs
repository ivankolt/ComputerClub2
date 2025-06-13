using ComputerClub.BD;
using System.Windows;
using System.Windows.Controls;

namespace ComputerClub.Users
{
    public partial class ClubCardView : UserControl
    {
        public ClubCardView()
        {
            InitializeComponent();
            LoadUserCardInfo();
        }

        private void LoadUserCardInfo()
        {
            if (CurrentUser.Instance != null)
            {
                var dbManager = new DatabaseManager();
                var userInfo = dbManager.GetUserInfo(CurrentUser.Instance.Id);

                if (userInfo != null)
                {
                    CardNumberText.Text = $"{userInfo.CardNumber}";
                    BalanceText.Text = $"Баланс: {userInfo.Balance:C}";
                }
                else
                {
                    CardNumberText.Text = "Данные не найдены";
                    BalanceText.Text = string.Empty;
                }
            }
            else
            {
                CardNumberText.Text = "Пользователь не авторизован";
                BalanceText.Text = string.Empty;
            }
        }
    }
}