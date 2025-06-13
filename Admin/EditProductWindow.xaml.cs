using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ComputerClub.BD;
using Microsoft.Win32;

namespace ComputerClub.Admin
{
    public partial class EditProductWindow : Window
    {
        private readonly DatabaseManager _databaseManager = new DatabaseManager();
        private readonly Product _product;
        private string _selectedImagePath;
        private bool _imageChanged = false;

        public EditProductWindow(Product product)
        {
            InitializeComponent();
            _product = product;
            
            txtProductName.Text = product.ProductName;
            txtPrice.Text = product.Price.ToString();

            if (!string.IsNullOrEmpty(product.FullImagePath))
            {
                try
                {
                    string fullPath = product.FullImagePath;
                    if (File.Exists(fullPath))
                    {
                        BitmapImage image = new BitmapImage(new Uri(fullPath));
                        imgProduct.Source = image;
                        txtCurrentImage.Text = Path.GetFileName(product.FullImagePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                _imageChanged = true;
                
                BitmapImage image = new BitmapImage(new Uri(_selectedImagePath));
                imgProduct.Source = image;
                txtCurrentImage.Text = Path.GetFileName(_selectedImagePath);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProductName.Text))
                {
                    MessageBox.Show("Пожалуйста, введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректную цену товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string newImagePath = _product.FullImagePath;

                if (_imageChanged && !string.IsNullOrEmpty(_selectedImagePath))
                {
                    string fileName = Path.GetFileName(_selectedImagePath);
                    string destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                    File.Copy(_selectedImagePath, destinationPath, true);

                    newImagePath = Path.Combine("Images", "Products", fileName);
                }

                bool success = await _databaseManager.UpdateProduct(
                    _product.Id,
                    txtProductName.Text.Trim(),
                    price,
                    newImagePath
                );

                if (success)
                {
                    MessageBox.Show("Товар успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось обновить товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
