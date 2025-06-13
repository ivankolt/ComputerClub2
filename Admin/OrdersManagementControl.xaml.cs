using ComputerClub.BD;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComputerClub.Users;
using System.Data;

namespace ComputerClub.Admin
{
    public partial class OrdersManagementControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseManager _db = new DatabaseManager();

        public ObservableCollection<Order> PendingOrders { get; set; }
        public ObservableCollection<Booking> PendingBookings { get; set; }

        private bool _showOrders = true;
        public bool ShowOrders
        {
            get => _showOrders;
            set
            {
                if (_showOrders != value)
                {
                    _showOrders = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        LoadOrders();
                    }
                }
            }
        }

        private bool _showBookings = false;
        public bool ShowBookings
        {
            get => _showBookings;
            set
            {
                if (_showBookings != value)
                {
                    _showBookings = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        LoadBookings();
                    }
                }
            }
        }

        public ICommand ShowProductsCommand { get; }
        public ICommand ShowBookingsCommand { get; }
        public ICommand ShowPaymentsCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }

        public OrdersManagementControl()
        {
            InitializeComponent();
            DataContext = this;

            PendingOrders = new ObservableCollection<Order>();
            PendingBookings = new ObservableCollection<Booking>();


            ShowProductsCommand = new RelayCommand(() =>
            {
                ShowOrders = true;
                ShowBookings = false;

            });

            ShowBookingsCommand = new RelayCommand(() =>
            {
                ShowOrders = false;
                ShowBookings = true;

            });

            ShowPaymentsCommand = new RelayCommand(() =>
            {
                ShowOrders = false;
                ShowBookings = false;

                Payments payment = new Payments();
                payment.ShowDialog();
            });

            CompleteOrderCommand = new RelayCommand(ExecuteCompleteOrder, CanExecuteOrderAction);
            CancelOrderCommand = new RelayCommand(ExecuteCancelOrder, CanExecuteOrderAction);

            ConfirmBookingCommand = new RelayCommand(ExecuteConfirmBooking, CanExecuteBookingAction);
            CancelBookingCommand = new RelayCommand(ExecuteCancelBooking, CanExecuteBookingAction);

            if (ShowOrders)
            {
                LoadOrders();
            }
            else if (ShowBookings)
            {
                LoadBookings();
            }
        }

        private void LoadOrders()
        {
            try
            {
                PendingOrders.Clear();
                var ordersFromDb = _db.GetOrdersByStatus("Ожидает обработки");
                foreach (var order in ordersFromDb)
                {
                    PendingOrders.Add(order);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ожидающих заказов: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order selectedOrder)
            {
                var orderDetails = _db.GetOrderDetails(selectedOrder.Id);
                var userInfo = _db.GetUserFullInfo(selectedOrder.UserId);   

                var detailsWindow = new OrderDetailsWindow(orderDetails, userInfo, selectedOrder.TotalAmount);
                detailsWindow.ShowDialog();
            }
        }
        private void BookingsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BookingsGrid.SelectedItem is Booking selectedBooking)
            {
                var bookingDetails = _db.GetBookingDetails(selectedBooking.Id);
                var userInfo = _db.GetUserFullInfo(selectedBooking.UserId);

                var detailsWindow = new BookingDetailsWindow(bookingDetails, userInfo, selectedBooking);
                detailsWindow.ShowDialog();
            }
        }

        private void LoadBookings()
        {
            try
            {
                PendingBookings.Clear();
                var bookingsFromDb = _db.GetBookingsByStatus("Ожидаемый");
                foreach (var booking in bookingsFromDb)
                {
                    PendingBookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ожидающих бронирований: {ex.Message}",
                               "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanExecuteOrderAction(object parameter)
        {
            return parameter is Order;
        }
        private bool CanExecuteBookingAction(object parameter)
        {
            return parameter is Booking;
        }

        private void ExecuteCompleteOrder(object parameter)
        {
            if (parameter is Order orderToComplete)
            {
                try
                {
                    int paymentId = _db.CreatePaymentAndGetId(new Payment
                    {
                        Amount = orderToComplete.TotalAmount,
                        TypePayment = "покупка",
                        ServiceName = "Товар",
                        UserId = orderToComplete.UserId,
                        DatePayment = DateTime.Now
                    });

                    var orderDetails = _db.GetOrderDetails(orderToComplete.Id);

                    if (orderDetails.Rows.Count > 0)
                    {

                        var receiptData = new
                        {
                            payment = new
                            {
                                type = "покупка",
                                amount = orderToComplete.TotalAmount,
                                serviceName = "Товар",
                                orderId = orderToComplete.Id,
                                orderDate = orderToComplete.OrderDate,
                                employeeId = CurrentUser.Instance.EmployeeId,
                                employeeName = _db.GetEmployeeInfo(CurrentUser.Instance.EmployeeId)?.FullName ?? "Неизвестный сотрудник"
                            },
                            items = orderDetails.AsEnumerable().Select(item => new
                            {
                                name = item.Field<string>("product"),  
                                quantity = item.Field<int>("quantity"), 
                                price = item.Field<decimal>("price_per_unit"),  
                                total = item.Field<decimal>("price_per_unit") * item.Field<int>("quantity")
                            }).ToArray()
                        };

                       
                        _db.CreateReceipt(
                            paymentId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(receiptData)
                        );

                        _db.UpdateOrderStatus(orderToComplete.Id, "Готов к выдаче");
                        PendingOrders.Remove(orderToComplete);

                        MessageBox.Show($"Заказ №{orderToComplete.Id} успешно завершен и готов к выдаче.", "Заказ завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении заказа №{orderToComplete.Id}: {ex.Message}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteCancelOrder(object parameter)
        {
            if (parameter is Order orderToCancel)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите отменить заказ №{orderToCancel.Id}? Сумма {orderToCancel.TotalAmount:C} будет возвращена на баланс пользователя.",
                    "Подтверждение отмены заказа",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _db.CancelOrderWithTransaction(orderToCancel.Id, orderToCancel.UserId, orderToCancel.TotalAmount);

                        PendingOrders.Remove(orderToCancel);

                        MessageBox.Show($"Заказ №{orderToCancel.Id} успешно отменен. Средства возвращены.", "Заказ отменен", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене заказа №{orderToCancel.Id}: {ex.Message}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }



        private void ExecuteConfirmBooking(object parameter)
        {
            if (parameter is Booking bookingToConfirm)
            {
                try
                {                  
                    _db.UpdateBookingStatus(bookingToConfirm.Id, "Подтверждённый");
                    PendingBookings.Remove(bookingToConfirm);

                    int paymentId = _db.CreatePaymentAndGetId(new Payment
                    {
                        Amount = bookingToConfirm.TotalAmount,
                        TypePayment = "бронирование",
                        ServiceName = "Бронирование ПК",
                        UserId = bookingToConfirm.UserId,
                        DatePayment = DateTime.Now
                    });

                    var bookingDetailsTable = _db.GetBookingDetails(bookingToConfirm.Id);
                    
                    if (bookingDetailsTable.Rows.Count > 0)
                    {
                        var firstRow = bookingDetailsTable.Rows[0];

                        var receiptData = new
                        {
                            payment = new
                            {
                                type = "бронирование",
                                amount = bookingToConfirm.TotalAmount,
                                serviceName = "Бронирование ПК",
                                bookingId = bookingToConfirm.Id,
                                employeeId = CurrentUser.Instance.EmployeeId,
                                employeeName = _db.GetEmployeeInfo(CurrentUser.Instance.EmployeeId)?.FullName ?? "Неизвестный сотрудник"
                            },
                            bookingDetails = new
                            {
                                pcId = Convert.ToInt32(firstRow["ПК"]),
                                pcZone = firstRow["Зона"].ToString(),
                                startTime = bookingToConfirm.StartTime,
                                endTime = bookingToConfirm.EndTime,
                                duration = (bookingToConfirm.EndTime - bookingToConfirm.StartTime).TotalHours
                            }
                        };
                        
            
                        _db.CreateReceipt(
                            paymentId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(receiptData)
                        );

                        MessageBox.Show($"Бронирование №{bookingToConfirm.Id} успешно подтверждено.", "Бронирование подтверждено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось найти детали бронирования №{bookingToConfirm.Id}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подтверждении бронирования №{bookingToConfirm.Id}: {ex.Message}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteCancelBooking(object parameter)
        {
            if (parameter is Booking bookingToCancel)
            {
                var result = MessageBox.Show(
                   $"Вы действительно хотите отменить бронирование №{bookingToCancel.Id}? Сумма {bookingToCancel.TotalAmount:C} будет возвращена на баланс пользователя.",
                   "Подтверждение отмены бронирования",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _db.CancelBookingWithTransaction(bookingToCancel.Id, bookingToCancel.UserId, bookingToCancel.TotalAmount);

                        PendingBookings.Remove(bookingToCancel);

                        MessageBox.Show($"Бронирование №{bookingToCancel.Id} успешно отменено. Средства возвращены.", "Бронирование отменено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене бронирования №{bookingToCancel.Id}: {ex.Message}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

public class Booking : INotifyPropertyChanged
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public int UserId { get; set; }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(CanCancel));
        }
    }

    public bool IsPending => Status == "Ожидаемый";
    public bool CanCancel => Status == "Ожидаемый";

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Order : INotifyPropertyChanged
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int UserId { get; set; }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(CanCancel));
        }
    }

    public bool IsPending => Status == "Ожидает обработки";
    public bool CanCancel => Status == "Ожидает обработки";

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
