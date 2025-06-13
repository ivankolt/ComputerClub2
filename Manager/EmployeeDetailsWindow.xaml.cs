using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ComputerClub.Manager
{
    public partial class EmployeeDetailsWindow : Window
    {
        public EmployeeDetailsWindow(EmployeeFullInfo employeeDetails)
        {
            InitializeComponent();
            
            txtFirstName.Text = employeeDetails.FirstName;
            txtLastName.Text = employeeDetails.LastName;
            txtPhoneNumber.Text = employeeDetails.PhoneNumber;
            txtGender.Text = employeeDetails.Gender == "М" ? "Мужской" : "Женский";
            txtPosition.Text = employeeDetails.Position;
            txtSalary.Text = $"{employeeDetails.Salary:C}";
            txtHireDate.Text = employeeDetails.HireDate.ToString("dd.MM.yyyy");
            txtPassport.Text = $"{employeeDetails.PassportSeries} {employeeDetails.PassportNumber}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class EmployeeFullInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public System.DateTime HireDate { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
    }
}
