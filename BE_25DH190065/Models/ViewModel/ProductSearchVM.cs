using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace BE_25DH190065.Models.ViewModel
{
    public class ProductSearchVM
    {
        // Tiêu chí tìm kiếm theo từ khóa (tên, mô tả, danh mục)
        public string SearchTerm { get; set; }

        // Tiêu chí tìm kiếm theo khoảng giá
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Thứ tự sắp xếp
        public string SortOrder { get; set; }

        // Thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 5;

        // Danh sách sản phẩm thỏa điều kiện phân trang
        public IPagedList<Product> Products { get; set; }
    }
}