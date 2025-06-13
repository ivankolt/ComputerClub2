using System;
using System.Net.Http.Headers;

namespace ComputerClub.Admin
{
    public class PaymentModel
    {
        public int Id { get; set; }
        public DateTime DatePayment { get; set; }
        public decimal Amount { get; set; }
        public string TypePayment { get; set; }
        public string ServiceName { get; set; }
        public string Username { get; set; }
        public string AccountNumber { get; set; }
        public int UserId { get; set; }
    }
}
