using ComputerClub.BD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ComputerClub.BD.DatabaseManager;
using System.Windows.Threading;
using ComputerClub.Users;
using System.Runtime.CompilerServices;
using ComputerClub.Entrance;

namespace ComputerClub.Admin
{
    public partial class ShiftControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        private DispatcherTimer _timer;
        private ShiftInfo _currentShift;
        private EmployeeInfo _employee;

        public EmployeeInfo Employee
        {
            get => _employee;
            set { _employee = value; OnPropertyChanged(); }
        }

        public string ShiftStatus { get; private set; }
        public string TimerDisplay { get; private set; }
        public Brush StatusBackground { get; private set; } = Brushes.Gray;

        public bool CanStartShift => _currentShift == null || _currentShift.EndTime.HasValue;
        public bool CanEndShift => _currentShift != null && !_currentShift.EndTime.HasValue;

        public ShiftControl(int employeeId)
        {
            InitializeComponent();
            LoadData(employeeId);
            DataContext = this;
        }

        private void LoadData(int employeeId)
        {
            Employee = _db.GetEmployeeInfo(employeeId);
            _currentShift = _db.GetCurrentShift(employeeId);
            UpdateShiftStatus();
            SetupTimer();
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => UpdateTimer();
            _timer.Start();
        }

        private void UpdateTimer()
        {
            if (_currentShift != null && !_currentShift.EndTime.HasValue)
            {
                TimerDisplay = $"Прошло: {_currentShift.Duration:hh\\:mm\\:ss}";
                OnPropertyChanged(nameof(TimerDisplay));
            }
        }

        private void UpdateShiftStatus()
        {
            if (_currentShift == null)
            {
                ShiftStatus = "Смена не начата";
                StatusBackground = Brushes.Gray;
            }
            else if (!_currentShift.EndTime.HasValue)
            {
                ShiftStatus = "Смена активна";
                StatusBackground = Brushes.Green;
            }
            else
            {
                ShiftStatus = $"Смена завершена ({_currentShift.EndTime:HH:mm})";
                StatusBackground = Brushes.Orange;
            }

            OnPropertyChanged(nameof(ShiftStatus));
            OnPropertyChanged(nameof(StatusBackground));
            OnPropertyChanged(nameof(CanStartShift));
            OnPropertyChanged(nameof(CanEndShift));
        }

        private void StartShift_Click(object sender, RoutedEventArgs e)
        {
            _db.StartShift(CurrentUser.Instance.EmployeeId);
            _currentShift = _db.GetCurrentShift(CurrentUser.Instance.EmployeeId);
            UpdateShiftStatus();
        }

        private void EndShift_Click(object sender, RoutedEventArgs e)
        {
            if (_currentShift != null)
            {
                _db.EndShift(_currentShift.Id);
                _currentShift = _db.GetCurrentShift(CurrentUser.Instance.EmployeeId);
                UpdateShiftStatus();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new Input();
            inputWindow.Show();

            var window = Window.GetWindow(this);
            window?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}