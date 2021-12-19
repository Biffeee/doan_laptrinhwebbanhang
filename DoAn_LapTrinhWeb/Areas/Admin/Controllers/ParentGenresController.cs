using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Model;
using PagedList;
using System.Web.Helpers;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class ParentGenresController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list thể loại
        public ActionResult ParentGIndex(string search,string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 9);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = _db.ParentGenres.Count(a => a.status == "2");
                var list = from a in _db.ParentGenres
                           where (a.status == "1"|| a.status == "0")
                           orderby a.id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.id.ToString().Contains(search) || s.name.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên 
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.name.ToString().Contains(search));
                    return View("ParentGIndex", list.ToPagedList(pageNumber, 50));
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
        public JsonResult GetParentGenresSearch(string Prefix)
        {
            var search = (from c in _db.ParentGenres
                where c.name.Contains(Prefix)
                orderby c.name ascending
                select new { c.name, c.image});
            return Json(search, JsonRequestBehavior.AllowGet);
        }

        //View list trash thể loại
        public ActionResult ParentGTrash(string search, string show,int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 9);
                var pageNumber = (page ?? 1);
                var list = from a in _db.ParentGenres
                           where a.status == "2"
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.id.ToString().Contains(search) || s.name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<Model.ParentGenres>)list.Where(s => s.name.ToString().Contains(search));
                    return View("ParentGTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Thông tin thể loại
        public ActionResult ParentGDetails(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + genre.name + "", "warning");
                    return RedirectToAction("ParentGIndex");
                }
                return View(genre);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View thêm thể loại
        public ActionResult ParentGCreate()
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                return View();
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý thêm thể loại
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult ParentGCreate(ParentGenres genre)
        {
            string slug = SlugGenerator.SlugGenerator.GenerateSlug(genre.name);
            try
           {
                var checkslug = _db.ParentGenres.Any(m => m.slug == slug);
                if (checkslug)
                {
                    genre.slug = slug + "-" + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + new Random().Next(1, 1000);
                }
                else
                {
                    genre.slug = SlugGenerator.SlugGenerator.GenerateSlug(genre.name);
                }
                genre.status = genre.status;
                genre.create_at = DateTime.Now;
                genre.create_by = User.Identity.GetEmail();
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                if (genre.image != null)
                {
                    genre.image = genre.image;
                }
                else
                {
                    genre.image = "/Images/ImagesCollection/no-image-available.png";
                }
                genre.description = genre.description;
                _db.ParentGenres.Add(genre);
                _db.SaveChanges();
                Notification.set_flash("Thêm thành công: " + genre.name + "", "success");
                return RedirectToAction("ParentGIndex");
            }
           catch
           {
               Notification.set_flash("Lỗi", "danger");
           }

           return View(genre);
        }
        //View chỉnh sửa thể loại
        public ActionResult ParentGEdit(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("ParentGEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (genre.status == "1" || genre.status == "0"))
            {
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + genre.name + "", "warning");
                    return RedirectToAction("ParentGIndex");
                }
                return View(genre);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý chỉnh sửa thể loại
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]  
        public ActionResult ParentGEdit(ParentGenres genre,string returnUrl)
        {
            try
            {
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                genre.status = genre.status;
                _db.Entry(genre).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Cập nhật thành công: " + genre.name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("ParentGIndex");
            }
            catch
            {
                Notification.set_flash("404!", "warning");
            }
            return View(genre);
        }
        //Vô hiệu hóa thể loại
        public ActionResult DelTrash(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);

                var list = from a in _db.ParentGenres
                    join b in _db.Genres on a.id equals b.parent_genre_id
                    where b.parent_genre_id == id
                    select b;

                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + genre.name + "", "warning");
                    return RedirectToAction("ParentGIndex");
                }
                genre.status = "2";
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                _db.Entry(genre).State = EntityState.Modified;

                foreach (var listchildgenre in list)
                {
                    listchildgenre.status = "2";
                }
                _db.SaveChanges();
                Notification.set_flash("Đã chuyển: " + genre.name + " vào thùng rác!", "success");
                return RedirectToAction("ParentGIndex");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Khôi phục thể loại
        public ActionResult Undo(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);
                var list = from a in _db.ParentGenres
                    join b in _db.Genres on a.id equals b.parent_genre_id
                    where b.parent_genre_id == id
                    select b;

                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.name + "", "warning");
                    return RedirectToAction("ParentGIndex");
                }
                genre.status = "1";
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                _db.Entry(genre).State = EntityState.Modified;
                foreach (var listchildgenre in list)
                {
                    listchildgenre.status = "0";
                }

                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công thể loại: "+genre.name + "", "success");
                return RedirectToAction("ParentGTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa thể loại
        public ActionResult ParentGDelete(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("ParentGDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);
                if (genre == null)
                {
                    Notification.set_flash("Không tồn tại: " + genre.name + "", "warning");
                    return RedirectToAction("ParentGTrash");
                }
                return View(genre);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xác nhận xóa thể loại
        [HttpPost]
        [ActionName("ParentGDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.ParentGenres.SingleOrDefault(a => a.id == id);
                _db.ParentGenres.Remove(genre);
                _db.SaveChanges();
                Notification.set_flash("Đã xoá vĩnh viễn: " + genre.name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("ParentGTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public JsonResult ChangeStatus(int id, int state = 0)
        {
            ParentGenres parent = _db.ParentGenres.Where(m => m.id == id).FirstOrDefault();
            string title = parent.name;
            parent.status = state.ToString();
            string prefix = state.ToString() == "1" ? "Hiển thị" : "Không hiển thị";
            parent.update_at = DateTime.Now;
            parent.update_by = User.Identity.GetEmail();
            _db.SaveChanges();
            return Json(new { Message = "Đã chuyển " + "\"" + title + "\"" + " sang " + prefix }, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}