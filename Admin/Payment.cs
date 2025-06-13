using System;

public class Payment
{
    public int Id { get; set; }
    public DateTime DatePayment { get; set; }
    public decimal Amount { get; set; }
    public string TypePayment { get; set; }
    public string ServiceName { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string AccountNumber { get; set; }
}
