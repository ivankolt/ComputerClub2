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
    public partial class UserDetailsWindow : Window
    {
        public UserDetailsWindow(UserFullInfo userDetails)
        {
            InitializeComponent();

            txtFirstName.Text = userDetails.FirstName;
            txtLastName.Text = userDetails.LastName;
            txtPhoneNumber.Text = userDetails.PhoneNumber;
            txtUsername.Text = userDetails.Username;
            txtEmail.Text = userDetails.Email;
            txtCardNumber.Text = userDetails.CardNumber;
            txtBalance.Text = $"{userDetails.Balance:C}";
            txtStatus.Text = userDetails.IsBlocked ? "Заблокирован" : "Активен";

            if (userDetails.IsBlocked && userDetails.BlockInfo != null)
            {
                blockInfoPanel.Visibility = Visibility.Visible;
                txtBlockReason.Text = userDetails.BlockInfo.Reason;
                txtBlockDate.Text = userDetails.BlockInfo.BlockDate.ToString("dd.MM.yyyy");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class UserFullInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string CardNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }
        public BlockInfo BlockInfo { get; set; }
    }

    public class BlockInfo
    {
        public string Reason { get; set; }
        public System.DateTime BlockDate { get; set; }
    }
}
