using BE_25DH190065.Models; // Khai báo namespace chứa MyStoreEntities
using System.Linq;
using BE_25DH190065.Models.ViewModel;
using PagedList;
using System.Web.Mvc;

namespace BE_25DH190065.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo Database Context
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm theo tên, mô tả hoặc tên danh mục
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDecription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            // Lấy 10 sản phẩm nổi bật (bán chạy nhất)
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            // Lấy danh sách sản phẩm mới nhất & phân trang
            int pageNumber = page ?? 1;
            int pageSize = 8;
            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductDetail/5
        public ActionResult ProductDetail (int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            // Lấy tất cả sản phẩm cùng danh mục (trừ sản phẩm hiện tại)
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailVM model = new ProductDetailVM();

            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            return View(model);
        }
    }
}