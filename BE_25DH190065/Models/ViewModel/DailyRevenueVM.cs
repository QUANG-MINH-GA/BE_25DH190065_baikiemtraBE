using System;

namespace BE_25DH190065.Models.ViewModel
{
    public class DailyRevenueVM
    {
        public DateTime OrderDay { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}