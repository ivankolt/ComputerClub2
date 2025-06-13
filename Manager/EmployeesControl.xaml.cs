using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComputerClub.BD;

namespace ComputerClub.Manager
{
    public partial class EmployeesControl : UserControl
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        private ObservableCollection<EmployeeInfo> _employees = new ObservableCollection<EmployeeInfo>();

        public EmployeesControl()
        {
            InitializeComponent();
            LoadEmployees();
            dgEmployees.ItemsSource = _employees;
        }

        private void LoadEmployees()
        {
            try
            {
                _employees.Clear();
                var employeesList = _db.GetAllEmployees();
                foreach (var employee in employeesList)
                {
                    _employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgEmployees.SelectedItem is EmployeeInfo selectedEmployee)
            {
                try
                {
                    var employeeDetails = _db.GetEmployeeDetails(selectedEmployee.Id);
                    var detailsWindow = new EmployeeDetailsWindow(employeeDetails);
                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var addEmployeeWindow = new AddEmployeeWindow();
            if (addEmployeeWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void btnFireEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmployees.SelectedItem is EmployeeInfo selectedEmployee)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите уволить сотрудника {selectedEmployee.FullName}?\n" +
                    "Это действие удалит все данные сотрудника из системы и не может быть отменено.",
                    "Подтверждение увольнения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _db.FireEmployee(selectedEmployee.Id);
                        
                        if (success)
                        {
                            MessageBox.Show("Сотрудник успешно уволен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadEmployees();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось уволить сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при увольнении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для увольнения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class EmployeeInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
    }
}
