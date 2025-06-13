using System;
using System.Collections.Generic;
using System.Data;
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

namespace ComputerClub.Admin
{
    public partial class BookingDetailsWindow : Window
    {
        public BookingDetailsWindow(DataTable bookingDetails, UserFullInfo userInfo, Booking booking)
        {
            InitializeComponent();
            DataContext = new
            {
                BookingDetails = bookingDetails.DefaultView,
                UserInfo = userInfo,
                Booking = booking
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
