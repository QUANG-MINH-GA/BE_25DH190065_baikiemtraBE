using BE_25DH190065.Models;
using BE_25DH190065.Models.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace BE_25DH190065.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên đăng nhập
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }


                // 1. Tạo tài khoản trong bảng Users với quyền Customer ("C")
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C"
                };
                db.Users.Add(user);

                // 2. Tạo thông tin khách hàng trong bảng Customers
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                db.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }
        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        // POST: Account/Login
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                string inputUser = (model.Username ?? "").Trim();
                string inputPass = (model.Password ?? "").Trim();

                // Tìm tài khoản khớp Username & Password (chấp nhận cả Khách hàng "C" và Admin "A")
                var user = db.Users.ToList().SingleOrDefault(u =>
                    u.Username != null &&
                    u.Username.Trim().Equals(inputUser, StringComparison.OrdinalIgnoreCase) &&
                    u.Password != null &&
                    u.Password.Trim() == inputPass);

                if (user != null)
                {
                    string role = (user.UserRole ?? "").Trim();
                    Session["Username"] = user.Username.Trim();
                    Session["UserRole"] = role;
                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.Username.Trim(), false);

                    // Nếu là tài khoản Admin ("A" hoặc "Admin") -> cấp luôn quyền Admin và chuyển vào trang Quản trị (hoặc Trang chủ)
                    if (role.Equals("A", StringComparison.OrdinalIgnoreCase) ||
                        role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["AdminUser"] = user.Username.Trim();
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    // Nếu là khách hàng ("C") -> về Trang chủ mua sắm
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
        // GET: Account/ProfileInfo (Hiển thị thông tin cá nhân)
        public ActionResult ProfileInfo()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new RegisterVM
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            };

            return View(model);
        }

        // POST: Account/UpdateProfile (Cập nhật Họ tên, SĐT, Email, Địa chỉ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(RegisterVM model)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer != null)
            {
                customer.CustomerName = model.CustomerName;
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerAddress = model.CustomerAddress;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            }

            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword (Đổi mật khẩu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmNewPassword)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();
            var user = db.Users.SingleOrDefault(u => u.Username == username);

            if (user == null || user.Password != OldPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không chính xác!";
                return RedirectToAction("ProfileInfo");
            }

            if (string.IsNullOrEmpty(NewPassword) || NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                return RedirectToAction("ProfileInfo");
            }

            user.Password = NewPassword;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ProfileInfo");
        }
    }
}
