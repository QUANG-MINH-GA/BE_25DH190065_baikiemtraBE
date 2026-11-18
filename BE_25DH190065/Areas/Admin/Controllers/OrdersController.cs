using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BE_25DH190065.Models;
using PagedList;

namespace BE_25DH190065.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders
        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            // 1. Tìm theo Tên khách hàng hoặc Tên đăng nhập
            if (!string.IsNullOrEmpty(customerName))
            {
                orders = orders.Where(o =>
                    o.Customer.CustomerName.Contains(customerName) ||
                    o.Customer.Username.Contains(customerName));
            }

            // 2. Tìm theo Trạng thái thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus.Contains(paymentStatus));
            }

            // 3. Tìm theo Ngày đặt hàng
            if (orderDate.HasValue)
            {
                DateTime startDate = orderDate.Value.Date;
                DateTime endDate = startDate.AddDays(1);
                orders = orders.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate);
            }

            ViewBag.CustomerName = customerName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";

            int pageNumber = page ?? 1;
            int pageSize = 15; // Phân trang 15 đơn hàng/trang theo yêu cầu đề

            // Sắp xếp mới nhất lên đầu theo cả ngày đặt lẫn mã đơn hàng
            var pagedOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToPagedList(pageNumber, pageSize);

            return View(pagedOrders);
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                          .Include(o => o.Customer)
                          .Include("OrderDetails.Product")
                          .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Include(o => o.Customer).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int OrderID, string PaymentStatus)
        {
            var order = db.Orders.Find(OrderID);
            if (order != null)
            {
                order.PaymentStatus = PaymentStatus;
                db.SaveChanges();
                return RedirectToAction("DetailedOrder", new { id = OrderID });
            }
            return HttpNotFound();
        }
    }
}