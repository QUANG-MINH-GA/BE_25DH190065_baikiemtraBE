using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using BE_25DH190065.Models;
using BE_25DH190065.Models.ViewModel;

namespace BE_25DH190065.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString().Trim();
            var customer = db.Customers.ToList().SingleOrDefault(c => c.Username.Trim() == username);
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

            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString().Trim();
            var customer = db.Customers.ToList().SingleOrDefault(c => c.Username.Trim() == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lưu thông tin chung vào bảng Orders
                    var order = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.TotalValue(),
                        PaymentStatus = model.PaymentMethod == "Tiền mặt" ? "COD" : "Chờ xử lý",
                        AddressDelivery = model.ShippingAddress
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    // 2. Lưu từng sản phẩm đã mua vào bảng OrderDetails
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

                    // 3. Làm trống giỏ hàng và chuyển sang trang Đặt hàng thành công
                    Session["Cart"] = null;
                    TempData["OrderSuccessMsg"] = "Đặt hàng thành công!";

                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"Lỗi cột [{ve.PropertyName}]: {ve.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "Lỗi CSDL: " + msg);
                }
            }

            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalValue();
            return View(model);
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Order/OrderHistory
        public ActionResult OrderHistory()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString().Trim();
            var customer = db.Customers.ToList().SingleOrDefault(c => c.Username.Trim() == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();
            return View(orders);
        }
        // POST: Order/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString().Trim();
            var customer = db.Customers.ToList().SingleOrDefault(c => c.Username.Trim() == username);
            var order = db.Orders.SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);

            if (order != null)
            {
                string status = (order.PaymentStatus ?? "").Trim();
                // Chỉ cho phép hủy khi đơn chưa hoàn tất thanh toán / chưa giao
                if (status != "Đã hủy" && status != "Đã giao" && status != "Đã thanh toán")
                {
                    order.PaymentStatus = "Đã hủy";
                    db.SaveChanges();
                    TempData["OrderMessage"] = $"Đã hủy thành công đơn hàng #{order.OrderID}.";
                }
            }

            return RedirectToAction("OrderHistory");
        }
        // GET: Order/MyOrder
        public ActionResult MyOrder(string statusTab = "Tất cả đơn", string searchTerm = "", bool showAll = false)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString().Trim();
            var customer = db.Customers.ToList().SingleOrDefault(c => c.Username.Trim() == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ordersQuery = db.Orders
                                .Include("OrderDetails.Product")
                                .Where(o => o.CustomerID == customer.CustomerID)
                                .AsQueryable();

            // 1. Tìm theo Mã đơn hàng hoặc Tên sản phẩm[cite: 11]
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string keyword = searchTerm.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderID.ToString().Contains(keyword) ||
                    o.OrderDetails.Any(od => od.Product.ProductName.Contains(keyword)));
            }

            // 2. Lọc theo Tab trạng thái[cite: 11]
            if (!string.IsNullOrEmpty(statusTab) && statusTab != "Tất cả đơn")
            {
                if (statusTab == "Đang xử lý")
                {
                    ordersQuery = ordersQuery.Where(o => o.PaymentStatus == "Đang xử lý" || o.PaymentStatus == "COD" || o.PaymentStatus == "Chờ xử lý");
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => o.PaymentStatus == statusTab);
                }
            }

            var allMatchedOrders = ordersQuery.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.StatusTab = statusTab;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ShowAll = showAll;
            ViewBag.TotalMatchedCount = allMatchedOrders.Count;

            // Mặc định mỗi trạng thái chỉ lấy 2 đơn gần nhất; bấm Xem thêm lấy tối đa 50 đơn[cite: 11]
            int limit = showAll ? 50 : 2;
            var resultOrders = (statusTab == "Tất cả đơn")
                ? allMatchedOrders
                : allMatchedOrders.Take(limit).ToList();

            return View(resultOrders);
        }
    }
}