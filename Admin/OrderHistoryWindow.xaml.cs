using ComputerClub.BD;
using System.Windows;

namespace ComputerClub.Admin
{
    public partial class OrderHistoryWindow : Window
    {
        public OrderHistoryWindow()
        {
            InitializeComponent();
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
            var databaseManager = new DatabaseManager();
            var history = databaseManager.GetOrderHistory();
            OrderHistoryGrid.ItemsSource = history.DefaultView;
        }
    }
}