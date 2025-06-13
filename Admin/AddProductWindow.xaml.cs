using ComputerClub.BD;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace ComputerClub.Admin
{
    public partial class AddProductWindow : Window
    {
        private string _imagePath;

        
        private readonly string _imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Images", "Products");

        public AddProductWindow()
        {
            InitializeComponent();
            LoadCategories();
            CreateImageFolder();
        }

        private void CreateImageFolder()
        {
            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }

        private void LoadCategories()
        {
            CategoryComboBox.ItemsSource = new[] { "БЕЗАЛКОГОЛЬНЫЕ_НАПИТКИ", "ЧИПСЫ_И_СНЕКИ", "ЭНЕРГЕТИЧЕСКИЕ_НАПИТКИ", "ИГРОВЫЕ_АКСЕССУАРЫ", "ШОКОЛАДНЫЕ_БАТОНЧИКИ" };
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _imagePath = openFileDialog.FileName;
                SelectedImageText.Text = Path.GetFileName(_imagePath);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    var destPath = CopyImageToFolder();

                    var newProduct = new Product
                    {
                        ProductName = ProductNameTextBox.Text,
                        Price = decimal.Parse(PriceTextBox.Text),
                        QuantityStore = 0,
                        Category = CategoryComboBox.SelectedItem.ToString(),
                        Picture = destPath
                    };

                    var dbManager = new DatabaseManager();
                    dbManager.AddProduct(newProduct);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string CopyImageToFolder()
        {
            string fileName = Path.GetFileName(_imagePath);
            
            var destPath = Path.Combine(_imageFolder, fileName);
            
            File.Copy(_imagePath, destPath, true);
            
            return fileName;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(_imagePath))
            {
                MessageBox.Show("Выберите изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}