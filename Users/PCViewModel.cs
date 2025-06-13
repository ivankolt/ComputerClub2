using System.Windows.Input;
using System.Windows;
using ComputerClub.BD;
using System;

namespace ComputerClub.Users
{
    public class PCViewModel
    {
        private readonly PC _pc;
        private readonly int _userId;
        private readonly DatabaseManager _databaseManager;

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;

        public string Status => _pc.Activity ? "Занят" : "Свободен";
        public bool IsAvailable => !_pc.Activity;
        public int PcId => _pc.Id;
        public string VideoCard => _pc.VideoCard;
        public string CPU => _pc.CPU;
        public decimal PricePerHour => _pc.PricePerHour;
        public string Monitor => _pc.Monitor;
        public int MonitorHertz => _pc.MonitorHertz;
        public string Keyboard => _pc.Keyboard;

        public ICommand BookCommand { get; }

        public PCViewModel(PC pc, int userId, DatabaseManager databaseManager)
        {
            _pc = pc;
            _userId = userId;
 
            _databaseManager = databaseManager;

            BookCommand = new RelayCommand(ExecuteBook, CanBook);
        }

        private void ExecuteBook()
        {
            DateTime currentTime = DateTime.Now;

            if (StartTime < currentTime)
            {
                MessageBox.Show("Время начала бронирования должно быть больше или равно текущему времени.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (StartTime >= EndTime)
            {
                MessageBox.Show("Время окончания бронирования должно быть позже времени начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int bookingId = _databaseManager.BookPC(_pc.Id, _userId, StartTime, EndTime);
                decimal totalAmount = _pc.PricePerHour * (decimal)(EndTime - StartTime).TotalHours;

                var confirmationWindow = new ConfirmationWindow(
                    bookingId,
                    _pc.Id,
                    _userId,
                    _pc.PricePerHour,
                    StartTime,
                    EndTime,
                    totalAmount);

                confirmationWindow.Owner = Application.Current.MainWindow;
                confirmationWindow.ShowDialog();

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool CanBook() => !_pc.Activity;
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? (param => true);
    }

    public RelayCommand(Action execute, Func<bool> canExecute = null)
        : this(_ => execute(), _ => canExecute?.Invoke() ?? true)
    {
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => _canExecute(parameter);

    public void Execute(object parameter) => _execute(parameter);
}