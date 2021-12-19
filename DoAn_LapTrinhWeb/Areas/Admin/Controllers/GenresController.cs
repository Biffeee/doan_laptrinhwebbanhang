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
    public class GenresController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list thể loại
        public ActionResult GenreIndex(string search,string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 9);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = _db.Genres.Count(a => a.status == "2");
                var list = from a in _db.Genres
                           where (a.status == "1"|| a.status == "0")
                           orderby a.genre_id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_id.ToString().Contains(search) || s.genre_name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_name.ToString().Contains(search));
                    return View("GenreIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View list trash thể loại
        public ActionResult GenreTrash(string search, string show,int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 9);
                var pageNumber = (page ?? 1);
                var list = from a in _db.Genres
                           where a.status == "2"
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_id.ToString().Contains(search) || s.genre_name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<Genre>)list.Where(s => s.genre_name.ToString().Contains(search));
                    return View("GenreTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Thông tin thể loại
        public ActionResult GenreDetails(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.genre_name + "", "warning");
                    return RedirectToAction("GenreIndex");
                }
                return View(genre);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View thêm thể loại
        public ActionResult GenreCreate()
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                ViewBag.ListParentGenre = new SelectList(_db.ParentGenres.Where(m => m.status == "1").OrderBy(m => m.name), "id", "name", 0);
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
        public ActionResult GenreCreate(Genre genre)
        {
            ViewBag.ListParentGenre = new SelectList(_db.ParentGenres.Where(m => m.status == "1").OrderBy(m => m.name), "id", "name", 0);
            string slug = SlugGenerator.SlugGenerator.GenerateSlug(genre.genre_name);
            try
            {
                var checkslug = _db.Genres.Any(m => m.slug == slug);
                if (checkslug)
                {
                    genre.slug = slug + "-" + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + new Random().Next(1, 1000);
                }
                else
                {
                    genre.slug = SlugGenerator.SlugGenerator.GenerateSlug(genre.genre_name);
                }
                genre.parent_genre_id = genre.parent_genre_id;
                genre.status = genre.status;
                genre.create_at = DateTime.Now;
                genre.create_by = User.Identity.GetEmail();
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                if (genre.genre_image != null)
                {
                    genre.genre_image = genre.genre_image;
                }
                else
                {
                    genre.genre_image = "/Images/ImagesCollection/no-image-available.png";
                }
                genre.description = genre.description;
                _db.Genres.Add(genre);
                _db.SaveChanges();
                Notification.set_flash("Thêm mới thể loại thành công thể loại: " + genre.genre_name + "", "success");
                return RedirectToAction("GenreIndex");
            }
           catch
           {
               Notification.set_flash("Lỗi", "danger");
           }

           return View(genre);
        }
        //View chỉnh sửa thể loại
        public ActionResult GenreEdit(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("GenreEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (genre.status == "1" || genre.status == "0"))
            {
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.genre_name + "", "warning");
                    return RedirectToAction("GenreIndex");
                }
                ViewBag.ListParentGenre = new SelectList(_db.ParentGenres.Where(m => m.status == "1").OrderBy(m => m.name), "id", "name", 0);
                return View(genre);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý chỉnh sửa thể loại
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]  
        public ActionResult GenreEdit(Genre genre,string returnUrl)
        {
            ViewBag.ListParentGenre = new SelectList(_db.ParentGenres.Where(m => m.status == "1").OrderBy(m => m.name), "id", "name", 0);
            try
            {
                genre.update_at = DateTime.Now;
                genre.parent_genre_id = genre.parent_genre_id;
                genre.update_by = User.Identity.GetEmail();
                genre.status = genre.status;
                _db.Entry(genre).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin thể loại: " + genre.genre_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("GenreIndex");
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
                var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.genre_name + "", "warning");
                    return RedirectToAction("GenreIndex");
                }
                genre.status = "2";
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                _db.Entry(genre).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã chuyển thể loại: " + genre.genre_name + " vào thùng rác!", "success");
                return RedirectToAction("GenreIndex");
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
                var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
                if (genre == null || id == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.genre_name + "", "warning");
                    return RedirectToAction("GenreIndex");
                }
                genre.status = "1";
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                _db.Entry(genre).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công thể loại: "+genre.genre_name+ "", "success");
                return RedirectToAction("GenreTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa thể loại
        public ActionResult GenreDelete(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("GenreDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
                if (genre == null)
                {
                    Notification.set_flash("Không tồn tại thể loại: " + genre.genre_name + "", "warning");
                    return RedirectToAction("GenreTrash");
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
        [ActionName("GenreDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var genre = _db.Genres.SingleOrDefault(a => a.genre_id == id);
                _db.Genres.Remove(genre);
                _db.SaveChanges();
                Notification.set_flash("Đã xoá vĩnh viễn thể loại: " + genre.genre_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("GenreTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}