using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BE_25DH190065.Models;
using BE_25DH190065.Models.ViewModel; // Đảm bảo đúng namespace chứa CartItem/CartService của bạn

namespace BE_25DH190065.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/MyOrder
        public ActionResult MyOrder(string status = "all", string search = "", string expandStatus = "")
        {
            // Kiểm tra đăng nhập (lấy Username từ Session)
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy toàn bộ đơn hàng của khách hàng hiện tại, sắp xếp mới nhất lên đầu
            var query = db.Orders.Where(o => o.CustomerID == customer.CustomerID);

            // 1. Tìm kiếm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderID.ToString().Contains(kw) ||
                    o.OrderDetails.Any(od => od.Product.ProductName.ToLower().Contains(kw))
                );
            }

            var allOrders = query.OrderByDescending(o => o.OrderDate).ToList();

            // Truyền dữ liệu trạng thái & tham số sang View MyOrder.cshtml
            ViewBag.CurrentStatus = status;
            ViewBag.SearchQuery = search;
            ViewBag.ExpandStatus = expandStatus;

            return View("MyOrder", allOrders);
        }

        // POST/GET: Nút MUA LẠI -> Thêm sản phẩm vào Giỏ hàng với số lượng = 1
        public ActionResult BuyAgain(int orderId)
        {
            var orderDetails = db.OrderDetails.Where(od => od.OrderID == orderId).ToList();
            if (orderDetails.Any())
            {
                // Lấy giỏ hàng hiện tại từ Session (khớp với CartService/Cart của dự án bạn)
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                foreach (var item in orderDetails)
                {
                    var product = db.Products.Find(item.ProductID);
                    if (product != null)
                    {
                        var existingItem = cart.FirstOrDefault(c => c.ProductID == product.ProductID);
                        if (existingItem != null)
                        {
                            existingItem.Quantity += 1; // Cộng thêm số lượng = 1
                        }
                        else
                        {
                            cart.Add(new CartItem
                            {
                                ProductID = product.ProductID,
                                ProductName = product.ProductName,
                                ProductImage = product.ProductImage,
                                UnitPrice = product.ProductPrice,
                                Quantity = 1 // Đúng yêu cầu: số lượng = 1
                            });
                        }
                    }
                }

                Session["Cart"] = cart;
            }

            // Chuyển sang trang Giỏ hàng để khách kiểm tra
            return RedirectToAction("Index", "Cart");
        }
    }
}