using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using DoAn_LapTrinhWeb;
using DoAn_LapTrinhWeb.Common;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.DTOs;
using DoAn_LapTrinhWeb.Models;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class RolesController : BaseController
    {
        private readonly DbContext db = new DbContext();
        //View list quản trị viên
        public ActionResult AdminIndex(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 10;
                var pageNumber = page ?? 1;

                ViewBag.countTrash =
                    db.Accounts.Count(a => a.status == "2" && a.Role == "0"); // đếm user status 0 và role 1
                var user = from a in db.Accounts
                           where (a.status == "1" || a.status == "0") && a.Role == "0"
                           orderby a.account_id descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("AdminIndex", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //View list biên tập viên
        public ActionResult EditorIndex(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 10;
                var pageNumber = page ?? 1;

                ViewBag.countTrash =
                    db.Accounts.Count(a => a.status == "2" && a.Role == "2"); // đếm user status 0 và role 2
                var user = from a in db.Accounts
                           where (a.status == "1" || a.status == "0") && a.Role == "2"
                           orderby a.account_id descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("EditorIndex", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //View list người dùng
        public ActionResult UserIndex(string search, string show, int? size, int? page)
        {
            var pageSize = size ?? 10;
            var pageNumber = page ?? 1;

            ViewBag.countTrash =
                db.Accounts.Count(a => a.status == "2" && a.Role == "1"); // đếm user status 0 và role 2
            var user = from a in db.Accounts
                       where (a.status == "1" || a.status == "0") && a.Role == "1"
                       orderby a.account_id descending // giảm dần
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                if (show.Equals("1"))//tìm kiếm tất cả
                    user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                    || s.status.Contains(search));
                else if (show.Equals("2"))//theo id
                    user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                else if (show.Equals("3"))//theo email
                    user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                else if (show.Equals("4"))//theo tên
                    user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                else if (show.Equals("5"))//theo số điện thoại
                    user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                else if (show.Equals("6"))//theo trạng thái
                    user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                return View("UserIndex", user.ToPagedList(pageNumber, 50));
            }
            return View(user.ToPagedList(pageNumber, pageSize));
        }
        //View list người kiểm duyệt
        public ActionResult ModeratorIndex(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 10;
                var pageNumber = page ?? 1;

                ViewBag.countTrash =
                    db.Accounts.Count(a => a.status == "2" && a.Role == "3"); // đếm user status 0 và role 2
                var user = from a in db.Accounts
                           where (a.status == "1" || a.status == "0") && a.Role == "3"
                           orderby a.account_id descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("ModeratorIndex", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //gợi ý tìm kiếm
        [HttpPost]
        public JsonResult GeUserSearch(string Prefix)
        {
            var search = (from c in db.Accounts
                          where c.status != "2" && c.Role=="1" && c.Email.StartsWith(Prefix)
                          orderby c.Email ascending
                          select new { c.Email });
            return Json(search, JsonRequestBehavior.AllowGet);
        }
        //Thông tin chi tiết tài khoản
        public ActionResult RolesDetails(int? id)
        {
            var user = (from a in db.Accounts
                        where a.account_id == id
                        orderby a.create_at descending // giảm dần
                        select a).FirstOrDefault();
            if (user != null && id != null) return View(user);
            Notification.set_flash("Không tồn tại tài khoản: " + user.Email + "", "warning");
            return RedirectToAction("Index", "Dashboard");
        }
        public ActionResult OrderHistory(int id, int? size, int? page,string sortOrder)
        {
            var pageSize = size ?? 10;
            var pageNumber = page ?? 1;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewBag.WaitingSortParm = sortOrder == "order_waiting" ? "order_waiting" : "order_waiting";
            ViewBag.ProcessingortParm = sortOrder == "order_processing" ? "order_processing" : "order_processing";
            ViewBag.CompleteSortParm = sortOrder == "order_complete" ? "order_complete" : "order_complete";
            ViewBag.CancleSortParm = sortOrder == "order_cancle" ? "order_cancle" : "order_cancle";
            var list = from a in db.Orders
                       orderby a.order_id descending // giảm dần
                       where a.account_id == id && a.Account.status == "1"
                       select new OrderHistoryDTOs
                       {
                           order_status = a.status,
                           order_id = a.order_id,
                           total_price = a.total,
                           create_at = a.create_at,
                           update_at = a.update_at,
                           order_note = a.order_note,
                       };
            switch (sortOrder)
            {
                case "date_desc":
                     list = from a in db.Orders
                                orderby a.order_id descending // giảm dần
                                where a.account_id == id && a.Account.status == "1"
                                select new OrderHistoryDTOs
                                {
                                    order_status = a.status,
                                    order_id = a.order_id,
                                    total_price = a.total,
                                    create_at = a.create_at,
                                    update_at = a.update_at,
                                    order_note = a.order_note
                                };
                    break;
                case "date_asc":
                    list = from a in db.Orders
                           orderby a.order_id ascending // giảm dần
                           where a.account_id == id && a.Account.status == "1"
                           select new OrderHistoryDTOs
                           {
                               order_status = a.status,
                               order_id = a.order_id,
                               total_price = a.total,
                               create_at = a.create_at,
                               update_at = a.update_at,
                               order_note = a.order_note
                           };
                    break;
                case "order_waiting":
                    list = from a in db.Orders
                           orderby a.order_id descending // giảm dần
                           where a.account_id == id && a.Account.status == "1" && a.status == "1"
                           select new OrderHistoryDTOs
                           {
                               order_status = a.status,
                               order_id = a.order_id,
                               total_price = a.total,
                               create_at = a.create_at,
                               update_at = a.update_at,
                               order_note = a.order_note
                           };
                    break;
                case "order_processing":
                    list = from a in db.Orders
                           orderby a.order_id descending // giảm dần
                           where a.account_id == id && a.Account.status == "1" && a.status == "2"
                           select new OrderHistoryDTOs
                           {
                               order_status = a.status,
                               order_id = a.order_id,
                               total_price = a.total,
                               create_at = a.create_at,
                               update_at = a.update_at,
                               order_note = a.order_note
                           };
                    break;
                case "order_complete":
                    list = from a in db.Orders
                           orderby a.order_id descending // giảm dần
                           where a.account_id == id && a.Account.status == "1" && a.status == "3"
                           select new OrderHistoryDTOs
                           {
                               order_status = a.status,
                               order_id = a.order_id,
                               total_price = a.total,
                               create_at = a.create_at,
                               update_at = a.update_at,
                               order_note = a.order_note
                           };
                    break;
                case "order_cancle":
                    list = from a in db.Orders
                           orderby a.order_id descending // giảm dần
                           where a.account_id == id && a.Account.status == "1" && a.status=="0"
                           select new OrderHistoryDTOs
                           {
                               order_status = a.status,
                               order_id = a.order_id,
                               total_price = a.total,
                               create_at = a.create_at,
                               update_at = a.update_at,
                               order_note = a.order_note
                           };
                    break;
                default:  // Name ascending 
                     list = from a in db.Orders
                            orderby a.order_id descending // giảm dần
                            where a.account_id == id && a.Account.status == "1"
                            select new OrderHistoryDTOs
                            {
                                order_status = a.status,
                                order_id = a.order_id,
                                total_price = a.total,
                                create_at = a.create_at,
                                update_at = a.update_at,
                                order_note = a.order_note
                            };
                    break;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }
        //View list trash biên tập viên
        public ActionResult EditorTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 5;
                var pageNumber = page ?? 1;
                var user = from a in db.Accounts
                           where a.status == "2" && a.Role == "2"
                           orderby a.update_at descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("EditorTrash", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //View list trash người kiểm duyệt
        public ActionResult ModeratorTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 5;
                var pageNumber = page ?? 1;
                var user = from a in db.Accounts

                           where a.status == "2" && a.Role == "3"
                           orderby a.update_at descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("ModeratorTrash", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //View list trash người dùng
        public ActionResult UserTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                var pageSize = size ?? 5;
                var pageNumber = page ?? 1;
                var user = from a in db.Accounts
                           where a.status == "2" && a.Role == "1"
                           orderby a.update_at descending // giảm dần
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.Phone.ToString().Contains(search)
                        || s.status.Contains(search));
                    else if (show.Equals("2"))//theo id
                        user = (IOrderedQueryable<Account>)user.Where(s => s.account_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo email
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Email.Contains(search));
                    else if (show.Equals("4"))//theo tên
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Name.Contains(search));
                    else if (show.Equals("5"))//theo số điện thoại
                        user = (IOrderedQueryable<Account>)user.Where(s => s.Phone.ToString().Contains(search));
                    else if (show.Equals("6"))//theo trạng thái
                        user = (IOrderedQueryable<Account>)user.Where(s => s.status.Contains(search));
                    return View("UserTrash", user.ToPagedList(pageNumber, 50));
                }
                return View(user.ToPagedList(pageNumber, pageSize));
            }
        }
        //View chỉnh sửa thông tin tài khoản
        public ActionResult RolesEdit(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("RolesEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            var account = db.Accounts.Where(x => x.account_id == id).SingleOrDefault();
            if (User.Identity.GetRole() != "0" && account.status == "2")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                //check xem có tồn tại id người dùng không
                if (account == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + account.Email + "", "warning");
                }
                //nếu không phải quyền admin "0" thì không cho phép chỉnh sửa thông tin cá nhân
                if (User.Identity.GetRole() != "0")
                {
                    //nếu không phải thì sẽ back về trang chủ admin
                    return RedirectToAction("Index", "Dashboard");
                }
                //tạo string url để back về trang trước đó, khi ấn nút lưu.

                return View(account);
            }
        }
        //Code xử lý chỉnh sửa thông tin tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RolesEdit(Account model, string returnUrl, int? id)
        {
            var account = db.Accounts.Where(m => m.account_id == id).SingleOrDefault();
            try
            {
                account.Avatar = model.Avatar;
                account.Name = model.Name;
                account.Email = model.Email;
                account.Role = model.Role;
                account.Phone = model.Phone;
                account.Dateofbirth = model.Dateofbirth;
                account.Gender = model.Gender;
                account.status = model.status;
                account.update_by = User.Identity.GetEmail();
                account.update_at = DateTime.Now;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                Notification.set_flash("Cập nhật thành công: " + account.Email + "", "success");
                //quay về trang trước đó khi ấn nút lưu nếu thông tin cập nhật thành công
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Dashboard");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }
            return View(model);
        }
        //Vô hiệu hóa tài khoản
        public ActionResult DelTrash(int? id,string returnUrl) // chuyển status về 0
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null
                && Request.UrlReferrer.ToString().Length > 0)
                {
                    return RedirectToAction("DelTrash", new { returnUrl = Request.UrlReferrer.ToString() });
                }
                var account = db.Accounts.SingleOrDefault(a => a.account_id == id);
                if (account == null || id == null)
                {
                    Notification.set_flash("Không tồn tại tài khoản: " + account.Email + "", "warning");
                    return RedirectToAction("Index");
                }
                account.status = "2";
                account.update_at = DateTime.Now;
                account.update_by = User.Identity.GetEmail();
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Đã vô hiệu hóa tài khoản: " + account.Email + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Dashboard");
            }
        }
        //Khôi phục tài khoản
        public ActionResult Undo(int? id,string returnUrl) // khôi phục từ thùng rác
        {
            if (User.Identity.GetRole() != "0")
            {
                //nếu không phải là admin thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null
                && Request.UrlReferrer.ToString().Length > 0)
                {
                    return RedirectToAction("Undo", new { returnUrl = Request.UrlReferrer.ToString() });
                }

                var account = db.Accounts.SingleOrDefault(a => a.account_id == id);
                if (account == null || id == null)
                {
                    Notification.set_flash("Không tồn tại tài khoản: " + account.Email + "", "warning");
                    return RedirectToAction("Index", "Dashboard");
                }
                account.status = "1";
                account.update_at = DateTime.Now;
                account.update_by = User.Identity.GetEmail();
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Khôi phục thành công tài khoản: " + account.Email + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa sản phẩm
        [HttpPost]
        public JsonResult RolesDelete(int id)
        {
            string result;
            var account = db.Accounts.FirstOrDefault(m => m.account_id == id);
            var newsProducts = db.NewsProducts.FirstOrDefault(a => a.product_id == id);
            var product_image = db.Product_Images.FirstOrDefault(m => m.product_id == id);

            if (account.Orders.Count > 0)
            {
                result = "ExitOrder";
            }
            else if (account.NewsComments.Count > 0)
            {
                result = "ExistComment";
            }
            else if (account.News.Count > 0)
            {
                result = "ExistPost";
            }
            else if (account.Feedbacks.Count > 0)
            {
                result = "ExistFeedback";
            }
            else
            {
                db.Accounts.Remove(account);
                db.SaveChanges();
                result = "Success";
                return Json(new { result, reload = true, Message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //View list quyền
        public ActionResult RolesList()
        {
            var admin = db.Accounts.Where(m=>m.Role=="0" && (m.status=="1"|| m.status == "0")).Count();
            ViewBag.countadmin = admin;
            var editor = db.Accounts.Where(m => m.Role == "2" && (m.status == "1" || m.status == "0")).Count();
            ViewBag.counteditor = editor;
            var moderator = db.Accounts.Where(m => m.Role == "3" && (m.status == "1" || m.status == "0")).Count();
            ViewBag.countmoderator = moderator;
            var user = db.Accounts.Where(m => m.Role == "1" && (m.status == "1" || m.status == "0")).Count();
            ViewBag.countuser = user;
            return View();
        }

        [HttpPost]
        public JsonResult ChangeStatus(int id, int state = 0)
        {
            Account account = db.Accounts.Where(m => m.account_id == id).FirstOrDefault();
            string title = account.Email;
            account.status = state.ToString();
            string prefix = state.ToString() == "1" ? "Hiển thị" : "Không hiển thị";
            account.update_at = DateTime.Now;
            account.update_by = User.Identity.GetEmail();
            db.SaveChanges();
            return Json(new { Message = "Đã chuyển " + "\"" + title + "\"" + " sang " + prefix }, JsonRequestBehavior.AllowGet);
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
