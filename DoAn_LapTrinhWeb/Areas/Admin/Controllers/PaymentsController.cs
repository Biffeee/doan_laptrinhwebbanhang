using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Model;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class PaymentsController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list phương thức thanh toán
        public ActionResult PaymentIndex(string search, string show, int? size, int? page)
        {
            var pageSize = (size ?? 9);
            var pageNumber = (page ?? 1);
            ViewBag.countTrash = _db.Payments.Count(a => a.status == "2");

            var list = from a in _db.Payments
                       where (a.status == "1" || a.status == "0")
                       orderby a.payment_id descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                if (show.Equals("1"))//tìm kiếm tất cả
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_id.ToString().Contains(search) || s.payment_name.Contains(search)
                    || s.create_by.Contains(search));
                else if (show.Equals("2"))//theo id
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_id.ToString().Contains(search));
                else if (show.Equals("3"))//theo tên thể loại
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_name.ToString().Contains(search));
                return View("PaymentIndex", list.ToPagedList(pageNumber, 50));
            }

            return View(list.ToPagedList(pageNumber, pageSize));
        }
        //View list trash phương thức thanh toán
        public ActionResult PaymentTrash(string search, string show, int? size, int? page)
        {
            var pageSize = (size ?? 9);
            var pageNumber = (page ?? 1);

            var list = from a in _db.Payments
                       where a.status == "2"
                       orderby a.update_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                if (show.Equals("1"))//tìm kiếm tất cả
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_id.ToString().Contains(search) || s.payment_name.Contains(search)
                    || s.create_by.Contains(search));
                else if (show.Equals("2"))//theo id
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_id.ToString().Contains(search));
                else if (show.Equals("3"))//theo tên thể loại
                    list = (IOrderedQueryable<Payment>)list.Where(s => s.payment_name.ToString().Contains(search));
                return View("PaymentTrash", list.ToPagedList(pageNumber, 50));
            }

            return View(list.ToPagedList(pageNumber, pageSize));
            //return View(_db.Payments.Where(m => m.status == "0").OrderByDescending(m => m.create_at).ToList());
        }
        //Thông tin phương thức thanh toán
        public ActionResult PaymentDetails(int? id)
        {
            var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
            if (payment == null || id == null)
            {
                Notification.set_flash("Không tồn tại: "+ payment.payment_name +"", "warning");
                return RedirectToAction("PaymentIndex");
            }

            return View(payment);
        }
        //View thêm phương thức thanh toán
        public ActionResult PaymentCreate()
        {
            return View();
        }
        //Code xử lý thêm phương thức thanh toán
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentCreate(Payment payment)
        {
            try
            {
                payment.create_at = DateTime.Now;
                payment.create_by = User.Identity.GetEmail();
                payment.update_at = DateTime.Now;
                payment.status = payment.status;
                payment.update_by = User.Identity.GetEmail();
                _db.Payments.Add(payment);
                _db.SaveChanges();
                Notification.set_flash("Đã Thêm phương thức thanh toán: " + payment.payment_name + "", "success");
                return RedirectToAction("PaymentIndex");
            }  catch
            {
                Notification.set_flash("Lỗi", "danger");
            }

            return View(payment);
        }
        //View chỉnh sửa thông tin phương thức thanh toán
        public ActionResult PaymentEdit(int? id)
        {
            var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (payment.status == "1"|| payment.status == "0"))
            {
                if (payment == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + payment.payment_name + "", "warning");
                    return RedirectToAction("PaymentIndex");
                }

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }

        }
        //Code xử lý chỉnh sửa thông tin phương thức thanh toán
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentEdit(Payment payment)
        {
            try
            {
                payment.update_at = DateTime.Now;
                payment.update_by = User.Identity.GetEmail();

                _db.Entry(payment).State = EntityState.Modified;
                _db.SaveChanges();

                Notification.set_flash("Đã cập nhật lại thông tin: " + payment.payment_name + "", "success");
                return RedirectToAction("PaymentIndex");
            }
            catch
            {
                Notification.set_flash("404!", "warning");
            }

            return View(payment);
        }
        //Vô hiệu hóa phương thức thanh toán
        public ActionResult DelTrash(int? id) //bỏ sp vào thùng rác
        {
            var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
            if (payment == null || id == null)
            {
                Notification.set_flash("Không tồn tại: " + payment.payment_name + "", "warning");
                return RedirectToAction("PaymentIndex");
            }
            payment.status = "2";
            payment.update_at = DateTime.Now;
            payment.update_by = User.Identity.GetEmail();
            _db.Entry(payment).State = EntityState.Modified;
            _db.SaveChanges();
            Notification.set_flash("Đã chuyển: " + payment.payment_name + " vào thùng rác", "success");
            return RedirectToAction("PaymentIndex");
        }
        //Khôi phục phương thức thanh toán
        public ActionResult Undo(int? id) // khôi phục từ thùng rác
        {
            var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
            if (payment == null || id == null)
            {
                Notification.set_flash("Không tồn tại: " + payment.payment_name + "", "warning");
                return RedirectToAction("PaymentIndex");
            }
            payment.status = "1";
            payment.update_at = DateTime.Now;
            payment.update_by = User.Identity.GetEmail();
            _db.Entry(payment).State = EntityState.Modified;
            _db.SaveChanges();
            Notification.set_flash("Khôi phục thành công: " + payment.payment_name + "", "success");
            return RedirectToAction("PaymentTrash");
        }
        //Xóa phương thức thanh toán
        public ActionResult PaymentDelete(int? id,string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("PaymentDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
                if (payment == null || id == null)
                {
                    Notification.set_flash("Không tồn tại! (ID = " + id + ")", "warning");
                    return RedirectToAction("PaymentTrash");
                }

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xác nhận xóa phương thức thanh toán
        [HttpPost]
        [ActionName("PaymentDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id, string returnUrl)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var payment = _db.Payments.SingleOrDefault(a => a.payment_id == id);
                _db.Payments.Remove(payment);
                _db.SaveChanges();
                Notification.set_flash("Xóa thành công: " + payment.payment_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("PaymentIndex");
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