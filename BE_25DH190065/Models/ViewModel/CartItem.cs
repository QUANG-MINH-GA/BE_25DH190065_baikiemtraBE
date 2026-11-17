namespace BE_25DH190065.Models.ViewModel
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }

        // Thành tiền của từng dòng sản phẩm
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}