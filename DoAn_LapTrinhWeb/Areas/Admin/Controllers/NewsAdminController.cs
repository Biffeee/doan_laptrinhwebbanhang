using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAn_LapTrinhWeb;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Models;
using PagedList;
using DoAn_LapTrinhWeb.DTOs;
using static DoAn_LapTrinhWeb.DTOs.NewsDTO;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class NewsAdminController : BaseController
    {
        private DbContext db = new DbContext();
        //view trang chủ danh sách tin tức
        public ActionResult NewsIndex(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = db.News.Count(a => a.status == "0");
                var list = from a in db.News
                           where (a.status == "1")
                           orderby a.news_id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_id.ToString().Contains(search) || s.news_title.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_title.ToString().Contains(search));
                    return View("NewsIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //view danh danh bài ghim
        public ActionResult StickyPost(string search, string show, int? size, int? page)
        {
            ViewBag.News = db.News.Where(m => m.status == "1").OrderByDescending(m => m.news_id).ToList();
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                List<StickyPost> sticky = db.StickyPosts.ToList();
                foreach(var item in sticky)
                {
                    if (item.sticky_end < DateTime.Now)
                    {
                        db.StickyPosts.Remove(item);
                        db.SaveChanges();
                    }
                }
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in db.StickyPosts
                           orderby a.id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<StickyPost>)list.Where(s => s.id.ToString().Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<StickyPost>)list.Where(s => s.id.ToString().Contains(search));
                    return View("NewsIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //xoá bài ghim
        [HttpPost]
        public JsonResult DeleteStickyPost(int id)
        {

            StickyPost hotPost = db.StickyPosts.Find(id);
            string tittle = hotPost.News.news_title;
            db.StickyPosts.Remove(hotPost);
            db.SaveChanges();
            return Json(new { reload = true, Message = "Gỡ bài ghim " + "ID"+id + " thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateStickyPost(int id,DateTime sticky_start, DateTime sticky_end)
        {
            StickyPost hotPost = new StickyPost
            {
                news_id = id,
                create_by = User.Identity.GetEmail(),
                update_by = User.Identity.GetEmail(),
                sticky_start = sticky_start,
                sticky_end = sticky_end,
            };
            db.StickyPosts.Add(hotPost);
            db.SaveChanges();
            return Json(new {reload = true, Message = "Ghim bài thành công" }, JsonRequestBehavior.AllowGet);
        }
        //kiểm tra bài viết ghim
        [HttpPost]
        public JsonResult checkPost(int id)
        {
            News news = db.News.Find(id);
            if (news != null)
            {
                return Json(new { valid = true, Message = news.news_title }, JsonRequestBehavior.AllowGet);
            }
            //Response.StatusCode = 500;
            return Json(new { valid = false, Message = "ID nhập không hợp lệ" }, JsonRequestBehavior.AllowGet);
        }
        //Danh sách bài viết trong thùng rác
        public ActionResult NewsTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in db.News
                           where (a.status == "0")
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_id.ToString().Contains(search) || s.news_title.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = (IOrderedQueryable<News>)list.Where(s => s.news_title.ToString().Contains(search));
                    return View("NewsTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xem chi tiết tin tức
        public ActionResult NewsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            ViewBag.newstags = db.NewsTags.Where(m => m.news_id == news.news_id).ToList();
            ViewBag.newsproducts = db.NewsProducts.Where(m => m.news_id == news.news_id).ToList();
            return View(news);
        }
        //thêm tin tức
        public ActionResult NewsCreate()
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                ViewBag.ListNewsCategory = new SelectList(db.ChildCategory.Where(m => (m.status == "1")).OrderBy(m => m.name), "childcategory_id", "name", 0);
                var newstagcheck = from t in db.Tags
                                   orderby t.tag_name ascending
                                   select new
                                   {
                                       t.tag_id,
                                       t.tag_name,
                                       Checked = ((from nt in db.NewsTags where (nt.tag_id == t.tag_id) select nt).Count() > 0)
                                   };
                var newsproductcheck = from p in db.Products
                                       where p.status=="1"
                                       orderby p.product_name ascending
                                       select new
                                       {
                                           p.product_id,
                                           p.genre_id,
                                           p.product_name,
                                           p.image,
                                           Checked = ((from np in db.NewsProducts where (np.product_id == p.product_id) && (np.genre_id == p.genre_id) select np).Count() > 0)
                                       };
                var MynewstagsCheckBoxList = new List<NewstagsCheckbox>();
                var MynewsproductCheckBoxList = new List<NewsProductsCheckbox>();
                foreach (var item in newstagcheck)
                {
                    MynewstagsCheckBoxList.Add(new NewstagsCheckbox { id = item.tag_id, name = item.tag_name });
                }
                foreach (var item in newsproductcheck)
                {
                    MynewsproductCheckBoxList.Add(new NewsProductsCheckbox { id = item.product_id,image=item.image, name = item.product_name, genre_id = item.genre_id });
                }
                var newsdto = new NewsDTO();
                newsdto.Tags = MynewstagsCheckBoxList;
                newsdto.Products = MynewsproductCheckBoxList;
                return View(newsdto);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //code xử lý thêm tin tức
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult NewsCreate(NewsDTO newsdto, News news)
        {
            ViewBag.ListNewsCategory = new SelectList(db.ChildCategory.Where(m => (m.status == "1")).OrderBy(m => m.name), "childcategory_id", "name", 0);
            string slug = SlugGenerator.SlugGenerator.GenerateSlug(newsdto.news_slug);
            try
            {

                if (newsdto.news_slug==null)
                {
                    news.slug = SlugGenerator.SlugGenerator.GenerateSlug(newsdto.meta_title);
                }
                else
                {
                    var checkslug = db.News.Any(m => m.slug == slug);
                    if (checkslug)
                    {
                        news.slug = slug + "-" + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + new Random().Next(1, 1000);
                    }
                    else
                    {
                        news.slug = SlugGenerator.SlugGenerator.GenerateSlug(newsdto.news_slug);
                    }
                }

                news.news_title = newsdto.news_title;
                news.news_content = newsdto.news_content;
                news.childcategory_id = newsdto.child_category_id;
                news.meta_title = newsdto.meta_title;
                news.ViewCount = 0;
                news.account_id = User.Identity.GetUserId();
                news.image = newsdto.image;
                news.image2 = newsdto.image2;
                news.create_at = DateTime.Now;
                news.status = newsdto.status;
                news.update_at = DateTime.Now;
                news.update_by = User.Identity.GetEmail();
                foreach (var item in newsdto.Tags)
                {
                    if (item.Checked)
                    {
                        db.NewsTags.Add(new NewsTags() { news_id = newsdto.news_id, tag_id = item.id });
                    }
                }
                foreach (var item in newsdto.Products)
                {
                    if (item.Checked)
                    {
                        db.NewsProducts.Add(new NewsProducts() { news_id = newsdto.news_id, product_id = item.id, genre_id = item.genre_id });
                    }
                }
                db.News.Add(news);
                db.SaveChanges();
                Notification.set_flash("Thêm thành công: " + news.news_title + "", "success");
                return RedirectToAction("NewsIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }
            return View(newsdto);
        }
        //Thêm nhanh thể loại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTags(Tags tags, NewsDTO newsdto)
        {
            string slug = SlugGenerator.SlugGenerator.GenerateSlug(newsdto.tag_name);
            try
            {
                var checkslug = db.Tags.Any(m => m.slug == slug);
                if (checkslug)
                {
                    tags.slug = slug + "-" + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + new Random().Next(1, 1000);
                }
                else
                {
                    tags.slug = SlugGenerator.SlugGenerator.GenerateSlug(newsdto.tag_name);
                }
                tags.tag_name = newsdto.tag_name;
                db.Tags.Add(tags);
                db.SaveChanges();
                Notification.set_flash("Thêm thành công: " + tags.tag_name + "", "success");
                return RedirectToAction("NewsCreate");
            }
            catch
            {
                Notification.set_flash("Thêm mới không thành công", "danger");
            }
            return View(tags);
        }
        //Sửa bài viết
        public ActionResult NewsEdit(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("NewsEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            News news = db.News.Where(m=>m.news_id==id).FirstOrDefault();
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (news.status == "1" || news.status == "0"))
            {
                if (news == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + news.news_title + "", "warning");
                    return RedirectToAction("NewsIndex");
                }
                ViewBag.ListNewsCategory = new SelectList(db.ChildCategory.Where(m => (m.status == "1")).OrderBy(m => m.name), "childcategory_id", "name", 0);
                var newstagcheck = from t in db.Tags
                                   orderby t.tag_name ascending
                             select new
                             {
                                 t.tag_id,
                                 t.tag_name,
                                 Checked = ((from nt in db.NewsTags where (nt.news_id == id) && (nt.tag_id == t.tag_id) select nt).Count() > 0)
                             };
                var newsproductcheck = from p in db.Products
                                       where p.status=="1"
                                       orderby p.product_name ascending
                                       select new
                                       {
                                           p.product_id,
                                           p.genre_id,
                                           p.product_name,
                                           p.image,
                                           Checked = ((from np in db.NewsProducts where (np.news_id == id) && (np.product_id == p.product_id) && (np.genre_id == p.genre_id) select np).Count() > 0)
                                       };
                var newsdto = new NewsDTO();
                newsdto.news_id = id.Value;
                newsdto.counttags = news.NewsTags.Count();
                newsdto.countproducts = news.NewsProducts.Count();
                newsdto.child_category_id = news.childcategory_id;
                newsdto.news_title = news.news_title;
                newsdto.meta_title = news.meta_title;
                newsdto.image = news.image;
                newsdto.image2 = news.image2;
                newsdto.news_content = news.news_content;
                newsdto.ViewCount = news.ViewCount;
                newsdto.status = news.status;
                var MynewstagsCheckBoxList = new List<NewstagsCheckbox>();
                var MynewsproductCheckBoxList = new List<NewsProductsCheckbox>();
                foreach (var item in newstagcheck)
                {
                    MynewstagsCheckBoxList.Add(new NewstagsCheckbox { id = item.tag_id, name = item.tag_name, Checked = item.Checked });
                }
                foreach (var item in newsproductcheck)
                {
                    MynewsproductCheckBoxList.Add(new NewsProductsCheckbox { id = item.product_id, name = item.product_name, image=item.image, genre_id = item.genre_id, Checked = item.Checked });
                }
                newsdto.Tags = MynewstagsCheckBoxList;
                newsdto.Products = MynewsproductCheckBoxList;
                return View(newsdto);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý chỉnh sửa bài viết
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult NewsEdit(NewsDTO newsdto, string returnUrl)
        {
            ViewBag.ListNewsCategory = new SelectList(db.ChildCategory.Where(m => (m.status == "1")).OrderBy(m => m.name), "childcategory_id", "name", 0);
            ViewBag.products = new MultiSelectList (db.Products.ToList());
            try
            {
                var news = db.News.Find(newsdto.news_id);
                news.news_title = newsdto.news_title;
                news.news_content = newsdto.news_content;
                news.image = newsdto.image;
                news.image2 = newsdto.image2;
                news.childcategory_id = newsdto.child_category_id;
                news.meta_title = newsdto.meta_title;
                news.update_at = DateTime.Now;
                news.update_by = User.Identity.GetEmail();
                news.status = newsdto.status;
                foreach(var item in db.NewsTags)
                {
                    if (item.news_id == newsdto.news_id)
                    {
                        db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                }
                foreach(var item in newsdto.Tags)
                {
                    if (item.Checked)
                    {
                        db.NewsTags.Add(new NewsTags() { news_id = newsdto.news_id, tag_id = item.id });
                    }
                }
                foreach (var item in db.NewsProducts)
                {
                    if (item.news_id == newsdto.news_id)
                    {
                        db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                }
                foreach (var item in newsdto.Products)
                {
                    if (item.Checked)
                    {
                        db.NewsProducts.Add(new NewsProducts() { news_id = newsdto.news_id, product_id = item.id, genre_id = item.genre_id });
                    }
                }
                db.SaveChanges();
                Notification.set_flash("Cập nhật thành thông: " + news.news_title + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("NewsIndex");
            }
            catch
            {
                Notification.set_flash("404!", "warning");
            }
            return View(newsdto);
        }
        //Xóa bài viết
        [HttpPost]
        public JsonResult NewsDelete(int id)
        {
            string result;
            var news = db.News.FirstOrDefault(m => m.news_id == id);
            var newstag = db.NewsTags.FirstOrDefault(a => a.news_id == id);
            var newsproduct = db.NewsProducts.FirstOrDefault(m => m.news_id == id);
            if (news.StickyPosts.Count > 0)
            {
                result = "ExitSticky";
            }
            else
            {
                if (news.NewsTags.Count > 0)
                {
                    db.NewsTags.Remove(newstag);
                    db.SaveChanges();
                }
                //xóa product image trước vì dính có id của product_id trong product_image
                if (news.NewsProducts.Count > 0)
                {
                    db.NewsProducts.Remove(newsproduct);
                    db.SaveChanges();
                }
                db.News.Remove(news);
                db.SaveChanges();
                result = "Success";
                return Json(new { result, reload = true, Message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //Thay đổi trạng thái bài viết
        [HttpPost]
        public JsonResult ChangeStatus(int id, int state = 0)
        {
            News news = db.News.Where(m => m.news_id == id).FirstOrDefault();
            int title = news.news_id;
            news.status = state.ToString();
            string prefix = state.ToString() == "1" ? "Hiển thị" : "Không hiển thị";
            news.update_at = DateTime.Now;
            news.update_by = User.Identity.GetEmail();
            db.SaveChanges();
            return Json(new { Message = "Đã chuyển ID" + title + " sang " + prefix }, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
