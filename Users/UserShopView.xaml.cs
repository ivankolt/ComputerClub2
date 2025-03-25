// UserShopView.xaml.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ComputerClub.BD;

namespace ComputerClub.Users
{
    public partial class UserShopView : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();

        private decimal _totalCartAmount;
        public decimal TotalCartAmount
        {
            get => _totalCartAmount;
            private set
            {
                _totalCartAmount = value;
                OnPropertyChanged(nameof(TotalCartAmount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public UserShopView()
        {
            InitializeComponent();
            DataContext = this;
            CartList.ItemsSource = CartItems;
            CartItems.CollectionChanged += (sender, e) =>
            {
                TotalCartAmount = CalculateTotal();
            };
            foreach (var item in CartItems)
            {
                item.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(CartItem.Quantity))
                        TotalCartAmount = CalculateTotal();
                };
            }
            CartItems.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var newItem in e.NewItems.OfType<CartItem>())
                    {
                        newItem.PropertyChanged += (s, ev) =>
                        {
                            if (ev.PropertyName == nameof(CartItem.Quantity))
                                TotalCartAmount = CalculateTotal();
                        };
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var oldItem in e.OldItems.OfType<CartItem>())
                    {
                        oldItem.PropertyChanged -= OnCartItemPropertyChanged;
                    }
                }

                TotalCartAmount = CalculateTotal();
            };

            LoadProducts();
        }

        private void OnCartItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity))
                TotalCartAmount = CalculateTotal();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadProducts()
        {
            ProductsList.ItemsSource = _db.GetAvailableProducts();
        }

        private decimal CalculateTotal()
        {
            return CartItems.Sum(item => item.TotalPrice);
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var productId = (int)button.Tag;
            var product = _db.GetProductById(productId);

            if (product == null)
            {
                MessageBox.Show("Товар не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingItem = CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            try
            {
                if (!_db.ConfirmPurchase(CurrentUser.Instance.Id, TotalCartAmount, CartItems.ToList()))
                {
                    MessageBox.Show("Недостаточно средств на счету!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Заказ успешно оформлен!");
                CartItems.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is CartItem item)
            {
                item.Quantity++;
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is CartItem item && item.Quantity > 0)
            {
                item.Quantity--;

                if (item.Quantity == 0)
                {
                    int index = CartItems.IndexOf(item);
                    if (index != -1)
                    {
                        CartItems.RemoveAt(index);
                    }
                }
            }
        }
    }

    public class CartItem : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => Price * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}