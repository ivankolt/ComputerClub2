using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ComputerClub.BD;

namespace ComputerClub.Admin
{
    public partial class Shope : UserControl
    {
        private readonly DatabaseManager _databaseManager = new DatabaseManager();

        public Shope()
        {
            InitializeComponent();
            FetchProducts(); 
        }

        private async void FetchProducts()
        {
                var products = await _databaseManager.GetProducts(); 
                ProductsList.ItemsSource = products;    
        }
        
        private void UpdateProducts_Click(object sender, RoutedEventArgs e)
        {
            FetchProducts(); 
        }

        private async void ArchiveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                await _databaseManager.ExecuteStoredProcedure("CALL archive_product(@id)", productId);
                FetchProducts();
            }
        }

        private async void RestoreProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                await _databaseManager.ExecuteStoredProcedure("CALL restore_product(@id)", productId);
                FetchProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                // Find the product in the current list
                var products = ProductsList.ItemsSource as System.Collections.Generic.List<Product>;
                var product = products?.Find(p => p.Id == productId);
                
                if (product != null)
                {
                    var editWindow = new EditProductWindow(product);
                    if (editWindow.ShowDialog() == true)
                    {
                        FetchProducts(); // Refresh the list after editing
                    }
                }
            }
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