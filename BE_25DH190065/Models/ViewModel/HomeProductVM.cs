using System.Collections.Generic;
using PagedList;

namespace BE_25DH190065.Models.ViewModel
{
    public class HomeProductVM
    {
        // Từ khóa tìm kiếm
        public string SearchTerm { get; set; }

        // Danh sách sản phẩm nổi bật (bán chạy)
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới nhất (có phân trang)
        public IPagedList<Product> NewProducts { get; set; }
    }
}