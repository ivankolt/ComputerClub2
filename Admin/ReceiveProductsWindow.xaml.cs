using System.Windows;
using ComputerClub.BD;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ComputerClub.Admin
{
    public partial class ReceiveProductsWindow : Window
    {
        private readonly DatabaseManager _dbManager = new DatabaseManager();
        private ObservableCollection<Product> _products;
        private int _selectedProductId;
        private int _receivedCount;

        public ReceiveProductsWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadProducts();
        }

        private async void LoadProducts()
        {
            var products = await _dbManager.GetProducts();
            _products = new ObservableCollection<Product>(products);
            OnPropertyChanged(nameof(Products));
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        private void SaveReceivedProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProductId == 0)
            {
                MessageBox.Show("Выберите товар.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ReceivedCount <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _dbManager.AddReceivedProduct(SelectedProductId, ReceivedCount);
            if (success)
            {
                MessageBox.Show("Товар успешно принят!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int SelectedProductId
        {
            get => _selectedProductId;
            set
            {
                _selectedProductId = value;
                OnPropertyChanged(nameof(SelectedProductId));
            }
        }

        public int ReceivedCount
        {
            get => _receivedCount;
            set
            {
                _receivedCount = value;
                OnPropertyChanged(nameof(ReceivedCount));
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}