using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace BE_25DH190065.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        // Gom nhóm các sản phẩm trong giỏ theo Danh mục (Slide 33)
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        // Hỗ trợ phân trang sản phẩm tương tự
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 4;
        public IPagedList<Product> SimilarProducts { get; set; }

        // Thêm sản phẩm vào giỏ (lưu kèm Category)
        public void AddItem(int productId, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);
            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Category = category
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    RemoveItem(productId);
            }
        }

        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}