using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BE_25DH190065.Models;
using BE_25DH190065.Models.ViewModel;

namespace BE_25DH190065.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/MyOrder
        public ActionResult MyOrder(string status = "all", string search = "", string expandStatus = "")
        {
            string username = User.Identity.IsAuthenticated
                ? User.Identity.Name
                : (Session["Username"] != null ? Session["Username"].ToString() : null);

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = db.Orders.Where(o => o.CustomerID == customer.CustomerID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderID.ToString().Contains(kw) ||
                    o.OrderDetails.Any(od => od.Product.ProductName.ToLower().Contains(kw))
                );
            }

            var allOrders = query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.SearchQuery = search;
            ViewBag.ExpandStatus = expandStatus;

            return View("MyOrder", allOrders);
        }

        // GET: Order/BuyAgain -> Thêm sản phẩm vào Giỏ hàng với số lượng = 1
        public ActionResult BuyAgain(int orderId)
        {
            var orderDetails = db.OrderDetails.Where(od => od.OrderID == orderId).ToList();
            if (orderDetails.Any())
            {
                var cart = Session["Cart"] as Cart ?? new Cart();

                foreach (var item in orderDetails)
                {
                    var product = db.Products.Find(item.ProductID);
                    if (product != null)
                    {
                        string categoryName = product.Category != null ? product.Category.CategoryName : "";
                        cart.AddItem(product.ProductID, product.ProductImage, product.ProductName, product.ProductPrice, 1, categoryName);
                    }
                }

                Session["Cart"] = cart;
            }

            return RedirectToAction("Index", "Cart");
        }

        // GET: Order/Checkout
        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            string username = User.Identity.IsAuthenticated
                ? User.Identity.Name
                : (Session["Username"] != null ? Session["Username"].ToString() : null);

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            string username = User.Identity.IsAuthenticated
                ? User.Identity.Name
                : (Session["Username"] != null ? Session["Username"].ToString() : null);

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalValue(),
                PaymentStatus = "Chờ thanh toán",
                AddressDelivery = model.ShippingAddress
            };

            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart.Items)
            {
                var detail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                db.OrderDetails.Add(detail);
            }
            db.SaveChanges();

            Session["Cart"] = null;
            return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
        }

        // GET: Order/OrderSuccess/10 -> Trang xác nhận đặt hàng thành công riêng
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("OrderSuccess", order);
        }

        // GET/POST: Order/CancelOrder/10 -> Hủy đơn hàng đang Chờ thanh toán
        public ActionResult CancelOrder(int id)
        {
            string username = User.Identity.IsAuthenticated
                ? User.Identity.Name
                : (Session["Username"] != null ? Session["Username"].ToString() : null);

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer != null)
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
                if (order != null && (order.PaymentStatus == "Chờ thanh toán" || order.PaymentStatus == "COD"))
                {
                    order.PaymentStatus = "Đã huỷ";
                    db.SaveChanges();
                }
            }

            return RedirectToAction("MyOrder", "Order", new { status = "Đã huỷ" });
        }
    }
}