using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Models;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class BrandsController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list thương hiệu
        public ActionResult BrandIndex(string search,string show,int? size, int? page, string sortOrder)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = size ?? 10;
                var pageNumber = (page ?? 1);
                ViewBag.CurrentSort = sortOrder;
                ViewBag.ResetSort = string.IsNullOrEmpty(sortOrder) ? "" : "";
                ViewBag.DateSortParm = sortOrder == "date_desc" ? "date_asc" : "date_desc";
                ViewBag.NameSort = sortOrder == "name_desc" ? "name_asc" : "name_desc";
                ViewBag.countTrash = _db.Brands.Count(a => a.status == "0" || a.status == "2");
                var list = from a in _db.Brands
                           where (a.status == "1")
                           orderby a.brand_id descending
                           select a;
                switch (sortOrder)
                {
                    case "date_desc":
                        ViewBag.sortname = "Xếp theo: Mới nhất";
                        list = (from a in _db.Brands
                                where (a.status == "1")
                                orderby a.brand_id descending
                                select a);
                        break;

                    case "date_asc":
                        ViewBag.sortname = "Xếp theo: Cũ nhất";
                        list = from a in _db.Brands
                               where (a.status == "1")
                               orderby a.brand_id ascending
                               select a;
                        break;
                    case "name_desc":
                        ViewBag.sortname = "Xếp theo: Z-A";
                        list = from a in _db.Brands
                               where (a.status == "1")
                               orderby a.brand_name descending
                               select a;
                        break;
                    case "name_asc":
                        ViewBag.sortname = "Xếp theo: A-Z";
                        list = from a in _db.Brands
                               where (a.status == "1")
                               orderby a.brand_name ascending
                               select a;
                        break;
                }
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))
                    {
                        ViewBag.searchName = "Tìm kiếm tất cả";
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_id.ToString().Contains(search)
                        || s.brand_name.Contains(search));
                       

                    }//tìm kiếm tất cả

                    else if (show.Equals("2"))
                    {
                        ViewBag.searchName = "Tìm kiếm ID";
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_id.ToString().Contains(search));
                    }//theo id

                    else if (show.Equals("3")) {
                        ViewBag.searchName = "Tìm kiếm theo tên ";
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_name.Contains(search));
                    }//theo tên 
                      
                    return View("BrandIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public JsonResult GetBrandSearch(string Prefix)
        {
            var search = (from c in _db.Brands
                          where c.brand_name.Contains(Prefix)
                          orderby c.brand_name ascending
                          select new { c.brand_name, c.brand_image});
            return Json(search, JsonRequestBehavior.AllowGet);
        }

        //View list Trash thương hiệu
        public ActionResult BrandTrash(string search,string show,int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = _db.Brands.Count(a => a.status == "2");
                var list = from a in _db.Brands
                           where a.status == "2"
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_id.ToString().Contains(search) || s.brand_name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<Brand>)list.Where(s => s.brand_name.ToString().Contains(search));
                    return View("BrandTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Thông tin chi tiết thương hiệu
        public ActionResult BrandDetails(int? id)
        {
            var brand = _db.Brands.SingleOrDefault(a => a.brand_id == id);
            if (brand != null && id != null) return View(brand);
            Notification.set_flash("Không tồn tại thương hiệu: " + brand.brand_name + "", "warning");
            return RedirectToAction("BrandIndex");
        }
        //View thêm thương hiệu
        public ActionResult BrandCreate()
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                     return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý thêm thương hiệu
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult BrandCreate(Brand brand)
        {
            string slug = SlugGenerator.SlugGenerator.GenerateSlug(brand.brand_name);
            try
            {
                var checkslug = _db.Brands.Any(m => m.slug == slug);
                if (checkslug)
                {
                    brand.slug = slug + "-" + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + new Random().Next(1, 1000);
                }
                else
                {
                    brand.slug = SlugGenerator.SlugGenerator.GenerateSlug(brand.brand_name);
                }
                brand.create_at = DateTime.Now;
                brand.create_by = User.Identity.GetEmail();
                if (brand.brand_image!=null)
                {
                    brand.brand_image = brand.brand_image;
                }
                else
                {
                    brand.brand_image = "/Images/ImagesCollection/no-image-available.png";
                }
                brand.Web_directory = brand.Web_directory;
                brand.description = brand.description;
                brand.update_at = DateTime.Now; 
                brand.status = brand.status;
                brand.update_by = User.Identity.GetEmail();
                _db.Brands.Add(brand);
                _db.SaveChanges();
                Notification.set_flash("Đã thêm mới thương hiệu: " + brand.brand_name + "", "success");
                return RedirectToAction("BrandIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }
            return View(brand);
        }
        //View chỉnh sửa thông tin thương hiệu
        public ActionResult BrandEdit(int? id,string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("BrandEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            var brand = _db.Brands.SingleOrDefault(pro => pro.brand_id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (brand.status=="1" || brand.status == "0"))
            {
                if (brand != null)
                {
                    return View(brand);
                }
                else { 
                Notification.set_flash("Không tồn tại thương hiệu: " + brand.brand_name + "", "warning");
                return RedirectToAction("BrandIndex");
                }
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult BrandEdit(Brand brand, string returnUrl)
        {
            try
            {
                brand.Web_directory = brand.Web_directory;
                brand.update_at = DateTime.Now;
                brand.brand_image = brand.brand_image;
                brand.description = brand.description;
                brand.status = brand.status ;
                brand.update_by = User.Identity.GetEmail();
                _db.Entry(brand).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin thương hiệu: " + brand.brand_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("BrandIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }
            return View(brand);
        }
        public ActionResult DelTrash(int? id) 
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var brand = _db.Brands.SingleOrDefault(pro => pro.brand_id == id);
                if (brand == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thương hiệu: " + brand.brand_name + "", "warning");
                    return RedirectToAction("Index");
                }
                brand.status = "2";
                brand.update_at = DateTime.Now;
                brand.update_by = User.Identity.GetEmail();
                _db.Entry(brand).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã chuyển thương hiệu: " + brand.brand_name + " vào thùng rác", "success");
                return RedirectToAction("BrandIndex");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        public ActionResult Undo(int? id) 
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var brand = _db.Brands.SingleOrDefault(pro => pro.brand_id == id);
                if (brand == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thương hiệu: " + brand.brand_name + "", "warning");
                    return RedirectToAction("BrandIndex");
                }
                brand.status = "1";
                brand.update_at = DateTime.Now;
                brand.update_by = User.Identity.GetEmail();
                _db.Entry(brand).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công thương hiệu: " + brand.brand_name+ "", "success");
                return RedirectToAction("BrandTrash");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa thương hiệu
        [HttpPost]
        public JsonResult BrandDelete(int id)
        {
            Boolean result;
            Brand brand = _db.Brands.Find(id);
            if (brand.Products.Count > 0)
            {
                result = false;
            }
            else
            {
                _db.Brands.Remove(brand);
                _db.SaveChanges();
                result = true;
                return Json(new { result, reload = true, Message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}