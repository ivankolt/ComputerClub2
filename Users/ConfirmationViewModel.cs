// ConfirmationViewModel.cs
using System;
using System.Windows;
using System.Windows.Input;
using ComputerClub.BD;

namespace ComputerClub.Users
{
    public class ConfirmationViewModel
    {
        private readonly DatabaseManager _dbManager;
        private readonly int _bookingId;
        private readonly int _userId;
        private readonly decimal _totalAmount;

        public int PcId { get; }
        public decimal PricePerHour { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public double Hours => (EndTime - StartTime).TotalHours;
        public decimal TotalAmount => _totalAmount;

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ConfirmationViewModel(int bookingId, int pcId, int userId, decimal pricePerHour,
                                   DateTime startTime, DateTime endTime, decimal totalAmount)
        {
            _dbManager = new DatabaseManager();
            _bookingId = bookingId;
            _userId = userId;
            _totalAmount = totalAmount;

            PcId = pcId;
            PricePerHour = pricePerHour;
            StartTime = startTime;
            EndTime = endTime;

            ConfirmCommand = new RelayCommand(ConfirmBooking);
            CancelCommand = new RelayCommand(CancelBooking);
        }

        private void ConfirmBooking()
        {
            try
            {
                if (_dbManager.ConfirmBooking(_bookingId, _userId, _totalAmount))
                {
                    MessageBox.Show("Оплата прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                else
                {
                    MessageBox.Show("Недостаточно средств на счету!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оплаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBooking()
        {
            try
            {
                _dbManager.CancelBooking(_bookingId);
                MessageBox.Show("Бронирование отменено", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отмены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}