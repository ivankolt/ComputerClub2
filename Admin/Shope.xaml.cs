using System.Windows;
using System.Windows.Controls;
using ComputerClub.BD;

namespace ComputerClub.Admin
{
    public partial class Shope : UserControl
    {
        private readonly DatabaseManager _databaseManager = new DatabaseManager();

        public Shope()
        {
            InitializeComponent();
            FetchProducts(); // Загружаем товары при создании UserControl
        }

        private void FetchProducts()
        {
            var products = _databaseManager.GetProducts();
            ProductsList.ItemsSource = products; 
        }

        private void UpdateProducts_Click(object sender, RoutedEventArgs e)
        {
            FetchProducts(); // Просто обновляем список товаров
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                FetchProducts(); 
            }
        }

        private void FetchProducts_Click(object sender, RoutedEventArgs e)
        {
            var receiveWindow = new ReceiveProductsWindow();
            receiveWindow.ShowDialog();
        }

        private void ShowOrderHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new OrderHistoryWindow();
            historyWindow.ShowDialog();
        }
    }
}