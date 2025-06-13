using ComputerClub.Admin;
using ComputerClub.BD;
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

namespace ComputerClub.Users
{
    /// <summary>
    /// Логика взаимодействия для PCInfoWindow.xaml
    /// </summary>
    public partial class PCInfoWindow : Window
    {
        private readonly DatabaseManager _databaseManager = new DatabaseManager();
        public PCInfoWindow(PC pc, int userId)
        {
            InitializeComponent();
            StartDateTimePicker.Value = DateTime.Now;
            EndDateTimePicker.Value = DateTime.Now;

            DataContext = new PCViewModel(pc, userId, _databaseManager);
        }
    }
}
