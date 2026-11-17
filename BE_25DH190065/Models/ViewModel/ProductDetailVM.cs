using System.Collections.Generic;
using PagedList;

namespace BE_25DH190065.Models.ViewModel
{
    public class ProductDetailVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1;

        // Tính tạm tính = số lượng * đơn giá
        public decimal estimatedValue => quantity * product.ProductPrice;

        // Phân trang cho danh sách sản phẩm bán chạy (Top Deals)
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 3;

        // Danh sách sản phẩm cùng danh mục
        public List<Product> RelatedProducts { get; set; }

        // Danh sách sản phẩm bán chạy nhất (có phân trang)
        public IPagedList<Product> TopProducts { get; set; }
    }
}