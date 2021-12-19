using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Model;
using DoAn_LapTrinhWeb.Models;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class DiscountsController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list giảm giá
        public ActionResult DiscountIndex(string search, string show, int? size, int? page,string sortOrder)
        {
            //var code = "SN" + accounts.Dateofbirth.ToString("dd") + accounts.Dateofbirth.ToString("MM") + DateTime.Now.ToString("yyy")
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = size ?? 10;
                var pageNumber = (page ?? 1);
                ViewBag.CurrentSort = sortOrder;
                ViewBag.ResetSort = String.IsNullOrEmpty(sortOrder) ? "" : "";
                ViewBag.DateSortParm = sortOrder == "date_desc" ? "date_asc" : "date_desc";
                ViewBag.PriceSortParm = sortOrder == "price_desc" ? "price_asc" : "price_desc";
                ViewBag.ProductSortParm = sortOrder == "discount_product" ? "discount_product" : "discount_product";
                ViewBag.DiscountGlobal = sortOrder == "global" ? "global" : "global";
                ViewBag.CodeSortParm = sortOrder == "discount_code" ? "discount_code" : "discount_code";
                ViewBag.CodePercentSortParm = sortOrder == "discount_code_per" ? "discount_code_per" : "discount_code_per";
                ViewBag.countTrash = _db.Discounts.Count(a => a.status == "0");
                var list = from a in _db.Discounts
                           where (a.status == "1")
                           orderby a.disscount_id descending
                           select a;
                switch (sortOrder)
                {
                    case "date_desc":
                        ViewBag.sortname = "Xếp theo: Mới nhất";
                        list = from a in _db.Discounts
                               where (a.status == "1")
                               orderby a.disscount_id descending
                               select a;
                        break;
                    case "global":
                        ViewBag.sortname = "Xếp theo: Hiển thị toàn sàn";
                        list = from a in _db.Discounts
                               where (a.discount_global == "1")
                               orderby a.disscount_id descending
                               select a;
                        break;
                    case "date_asc":
                        ViewBag.sortname = "Xếp theo: Cũ nhất";
                        list = from a in _db.Discounts
                               where (a.status == "1")
                               orderby a.disscount_id ascending
                               select a;
                        break;
                    case "price_desc":
                        ViewBag.sortname = "Xếp theo: Giảm nhiều - nhiều";
                        list = from a in _db.Discounts
                               where (a.status == "1")
                               orderby a.discount_price descending
                               select a;
                        break;
                    case "price_asc":
                        ViewBag.sortname = "Xếp theo: Giảm ít - nhiều";
                        list = from a in _db.Discounts
                               where (a.status == "1")
                               orderby a.discount_price ascending
                               select a;
                        break;
                    case "discount_product":
                        ViewBag.sortname = "Xếp theo: Giảm trực tiếp sản phẩm";
                        list = from a in _db.Discounts
                               where ((a.status == "1") && a.discounts_type==1)
                               orderby a.disscount_id descending
                               select a;
                        break;
                    case "discount_code":
                        ViewBag.sortname = "Xếp theo: Code giảm giá theo giá tiền";
                        list = from a in _db.Discounts
                               where ((a.status == "1") && a.discounts_type == 2)
                               orderby a.disscount_id descending
                               select a;
                        break;
                   case "discount_code_per":
                        ViewBag.sortname = "Xếp theo: Code giảm giá theo phần trăm";
                        list = from a in _db.Discounts
                               where ((a.status == "1") && a.discounts_type == 3)
                               orderby a.disscount_id descending
                               select a;
                        break;
                    default:
                        list = from a in _db.Discounts
                               where (a.status == "1")
                               orderby a.disscount_id descending
                               select a;
                        break;
                }
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.disscount_id.ToString().Contains(search) || s.discount_price.ToString().Contains(search)
                        || s.discount_name.Contains(search) || s.discounts_code.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.disscount_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên chương trình giảm giá
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.discount_name.Contains(search));
                    else if (show.Equals("4"))//theo mức giảm
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.discount_price.ToString().Contains(search));
                    else if (show.Equals("5"))//theo mã chương trình giảm giá
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.discounts_code.Contains(search));
                    return View("DiscountIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View list trash giảm giá
        public ActionResult DiscountTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = _db.Discounts.Count(a => a.status == "0" && a.discounts_type==2);
                var list = from a in _db.Discounts
                           where a.status == "0" 
                           orderby a.disscount_id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.disscount_id.ToString().Contains(search) || s.discount_price.ToString().Contains(search)
                        || s.discount_name.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.disscount_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên chương trình giảm giá
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.discount_name.Contains(search));
                    else if (show.Equals("4"))//theo mức giảm
                        list = (IOrderedQueryable<Discount>)list.Where(s => s.discount_price.ToString().Contains(search));
                    return View("DiscountTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Thông tin giảm giá
        public ActionResult DiscountDetails(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var discount = _db.Discounts.SingleOrDefault(a => a.disscount_id == id);
                if (discount == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + discount.discount_name + "", "warning");
                    return RedirectToAction("DiscountIndex");
                }
                return View(discount);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View thêm giảm giá
        public ActionResult DiscountCreate()
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
        //Code xử lý thêm giảm giá
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult DiscountCreate(Discount discount)
        {
            try
            {
                discount.create_at = DateTime.Now;
                discount.create_by = User.Identity.GetEmail();
                discount.update_at = DateTime.Now;
                discount.discount_start = discount.discount_start;
                discount.discount_end = discount.discount_end;
                discount.discount_global = discount.discount_global;
                discount.update_by = User.Identity.GetEmail();
                var check_exist_dc = _db.Discounts.Any(m => m.discounts_code == discount.discounts_code && m.discounts_code != null);
                 if((discount.discounts_type ==2 || discount.discounts_type == 3) && discount.discounts_code == null)
                {
                    Notification.set_flash("Vui lòng nhập mã giảm giá", "danger");
                    return View(discount);
                }
                else if ((discount.discounts_type == 2 || discount.discounts_type == 3) && discount.quantity == null)
                {
                    Notification.set_flash("Vui lòng nhập số lượng", "danger");
                    return View(discount);
                }
                else if(discount.discounts_type == 3 && discount.discount_price > 100)
                {
                    Notification.set_flash("Mức giảm phải nhỏ hơn hoặc bằng 100", "danger");
                    return View(discount);
                }
                else if ((discount.discounts_type == 3) && discount.discount_max<1000)
                {
                    Notification.set_flash("Mức giảm tối đa tối thiểu 1,000₫", "danger");
                    return View(discount);
                }
                else if (check_exist_dc)
                {
                    Notification.set_flash("Mã giảm giá đã tồn tại", "danger");
                    return View(discount);
                }
                else
                {
                    if (discount.quantity != null)
                    {
                        discount.quantity = discount.quantity;
                    }
                    if (discount.discounts_code != null)
                    {
                        discount.discounts_code = discount.discounts_code.Trim();
                    }
                    if (discount.discounts_type == 2)
                    {
                        discount.discount_max = discount.discount_price;
                    }
                    else
                    {
                        discount.discount_max = discount.discount_max;
                    }
                    _db.Discounts.Add(discount);
                    _db.SaveChanges();
                    Notification.set_flash("Thêm mới thành công chương trình giảm giá: " + discount.discount_name + "", "success");
                }
                return RedirectToAction("DiscountIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi!", "danger");
            }
            return View(discount);
        }
        //View chỉnh sửa thông tin giảm giá
        public ActionResult DiscountEdit(int? id,string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("DiscountEdit", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            var discount = _db.Discounts.SingleOrDefault(a => a.disscount_id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (discount.status == "1" || discount.status == "0"))
            {
                if (discount != null && id != null) { return View(discount); }
                else { 
                Notification.set_flash("Không tồn tại: " + discount.discount_name + "", "warning");
                return RedirectToAction("DiscountIndex");
                }
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý chỉnh sửa thông tin giảm giá
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult DiscountEdit(Discount discount,string returnUrl)
        {
            try
            {
                discount.update_at = DateTime.Now;
                discount.update_by = User.Identity.GetEmail();
                discount.status = discount.status;
                discount.quantity = discount.quantity;
                discount.discount_end = discount.discount_end;
                discount.discount_global = discount.discount_global;
                _db.Entry(discount).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin: '" + discount.discount_name + "'", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("DiscountIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi!", "danger");
            }
            return View(discount);
        }
        //Khôi phục giảm giá
        public ActionResult Undo(int? id) // khôi phục từ thùng rác
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var discount = _db.Discounts.SingleOrDefault(a => a.disscount_id == id);
                if (discount == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + discount.discount_name + "", "warning");
                    return RedirectToAction("DiscountIndex");
                }
                discount.status = "1";
                discount.update_at = DateTime.Now;
                discount.update_by = User.Identity.GetEmail();
                _db.Entry(discount).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công: " + discount.discount_name + "", "success");
                return RedirectToAction("DiscountTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa giảm giá
        public ActionResult DiscountDelete(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("DiscountDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var discount = _db.Discounts.SingleOrDefault(a => a.disscount_id == id);
                if (discount == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + discount.discount_name + "", "warning");
                    return RedirectToAction("DiscountTrash");
                }
                return View(discount);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xác nhận xóa giảm giá
        [HttpPost]
        [ActionName("DiscountDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("DeleteConfirmed", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var discount = _db.Discounts.SingleOrDefault(a => a.disscount_id == id);
                _db.Discounts.Remove(discount);
                _db.SaveChanges();
                Notification.set_flash("Đã xoá vĩnh viễn: " + discount.discount_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("DiscountTrash");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public JsonResult SendMailBirthDayCode(Discount discount)
        {
            Boolean result = false;
            try
            {
                DateTime now = DateTime.Now;
                DateTime Startdate = new DateTime(now.Year, now.Month, 1);
                var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                DateTime LastDay = new DateTime(now.Year, now.Month, DaysInMonth);
                List<Account> account = _db.Accounts.ToList();
                if (account == null)
                {
                    account = new List<Account>();
                }
                foreach (Account accounts in account)
                {
                    string code = "SN" + accounts.Dateofbirth.ToString("dd") + accounts.Dateofbirth.ToString("MM") + DateTime.Now.Year + accounts.account_id;
                    int account_id =  Convert.ToInt32(code.Contains((accounts.account_id).ToString()));
                    //if (code.Contains((accounts.account_id).ToString()) && code.Contains((DateTime.Now.Year).ToString()))
                    //{
                    //    status1 = accounts.account_id.ToString();
                    //}
                    Discount checkcode = _db.Discounts.Where(m => m.discounts_code == code).SingleOrDefault();
                    Order order = _db.Orders.FirstOrDefault();
                    DateTime lastyear = DateTime.Now.AddYears(-1);
                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                    if (checkcode == null && accounts.Dateofbirth.Month == DateTime.Now.Month && accounts.create_at.Year <= lastyear.Year && accounts.Role == "1" && accounts.Orders.Count() > 0)
                    {
                       if (accounts.Orders.Sum(m => m.total) >= 50000000)
                        {
                            discount.discount_price = 500000;
                        }
                        else if ( accounts.Orders.Sum(m => m.total) >= 15000000)
                        {
                            discount.discount_price = 200000;
                        }
                        else if (accounts.Orders.Sum(m => m.total) >= 3000000)
                        {
                            discount.discount_price = 50000;
                        }
                        else
                        {
                            discount.discount_price = 20000;
                        }
                        discount.discounts_code = code;
                        discount.update_at = DateTime.Now;
                        discount.update_by = User.Identity.GetEmail();
                        discount.discount_name = "Code sinh nhật" + " - " + accounts.Email;
                        discount.discount_start = Startdate;
                        discount.discount_end = LastDay;
                        discount.discounts_type = 2;
                        discount.create_at = DateTime.Now;
                        discount.update_at = DateTime.Now;
                        discount.create_by = User.Identity.GetEmail();
                        discount.update_by = User.Identity.GetEmail();
                        discount.status = "1";
                        discount.quantity = "1";
                        discount.discount_global = "0";
                        string BirthDayCode = discount.discounts_code;
                        string EmailID = accounts.Email;
                        string startday = Startdate.ToString("dd/MM");
                        string endday = LastDay.ToString("dd/MM");
                        string discount_price = discount.discount_price.ToString("#,0", cul.NumberFormat) + "₫";
                        string name = accounts.Name;
                        string account_birthday = accounts.Dateofbirth.ToString("dd/MM");
                        SendVerificationLinkEmail(EmailID, BirthDayCode, startday, endday, discount_price, name, account_birthday, "SendBirthDayCode");
                        _db.Discounts.Add(discount);
                        _db.SaveChanges();
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
           
        }

        
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string BirthDayCode, string startday, string endday, string discount_price, string name,string account_birthday, string emailFor)
        {
            var fromEmail = new MailAddress(AccountEmail.UserEmail, AccountEmail.Name); // "username email-vd: vn123@gmail.com" ,"tên hiển thị mail khi gửi"
            var toEmail = new MailAddress(emailID);
            //nhập password của bạn
            var fromEmailPassword = AccountEmail.Password;
            string subject = "";
            //dùng body mail html , file template nằm trong thư mục "EmailTemplate/Text.cshtml"
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "MailBirthDayCode" + ".cshtml");
            if (emailFor == "SendBirthDayCode")
            {
                subject = "Code giảm giá sinh nhật " + emailID;
                body = body.Replace("{{AccountName}}",name);
                body = body.Replace("{{Discountcode}}", BirthDayCode);
                body = body.Replace("{{Startday}}", startday);
                body = body.Replace("{{Endday}}", endday);
                body = body.Replace("{{discountprice}}", discount_price);
                body = body.Replace("{{AccountBirthDay}}", account_birthday);
                
            }
            var smtp = new SmtpClient
            {
                Host = AccountEmail.Host, //tên mấy chủ nếu bạn dùng gmail thì đổi  "Host = "smtp.gmail.com"
                Port = 587,
                EnableSsl = true, //bật ssl
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [HttpPost]
        public JsonResult ChangeStatus(int id, int state = 0)
        {
            Discount discount = _db.Discounts.Where(m => m.disscount_id == id).FirstOrDefault();
            int title = discount.disscount_id;
            discount.status = state.ToString();
            string prefix = state.ToString() == "1" ? "Hiển thị" : "Không hiển thị";
            discount.update_at = DateTime.Now;
            discount.update_by = User.Identity.GetEmail();
            _db.SaveChanges();
            return Json(new { Message = "Đã chuyển " + "ID" + title + " sang " + prefix }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}