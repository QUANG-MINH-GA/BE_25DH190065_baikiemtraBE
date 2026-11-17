namespace BE_25DH190065.Models.ViewModel
{
    public class StatisticsVM
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal AveragePrice { get; set; }
    }
}