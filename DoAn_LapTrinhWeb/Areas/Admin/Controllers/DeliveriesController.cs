using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Model;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class DeliveriesController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list đơn vị vận chuyển
        public ActionResult DeliveryIndex(string search,string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);

                ViewBag.countTrash = _db.Deliveries.Count(a => a.status == "2"); //  đếm tổng sp có trong thùng rác

                var list = from a in _db.Deliveries
                           where (a.status == "1" || a.status == "0")
                           orderby a.delivery_id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_id.ToString().Contains(search) || s.delivery_name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên 
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_name.ToString().Contains(search));
                    return View("DeliveryIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View list trash đơn vị vận chuyển
        public ActionResult DeliveryTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in _db.Deliveries
                           where a.status == "2"
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_id.ToString().Contains(search) || s.delivery_name.Contains(search)
                        || s.create_by.Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên 
                        list = (IOrderedQueryable<Delivery>)list.Where(s => s.delivery_name.ToString().Contains(search));
                    return View("DeliveryTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Thông tin chi tiết đơn vị vận chuyển
        public ActionResult DeliveryDetails(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var delivery = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
                if (delivery != null && id != null) return View(delivery);
                Notification.set_flash("Không tồn tại: " + delivery.delivery_name + "", "warning");
                return RedirectToAction("DeliveryIndex");
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View thêm đơn vị vận chuyển
        public ActionResult DeliveryCreate()
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
        //Code xử lý thêm đơn vị vận chuyển
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult DeliveryCreate(Delivery delivery)
        {
            try
            {
                delivery.status = delivery.status;
                delivery.create_at = DateTime.Now;
                delivery.create_by = User.Identity.GetEmail();
                delivery.update_at = DateTime.Now;
                delivery.update_by = User.Identity.GetEmail();
                _db.Deliveries.Add(delivery);
                _db.SaveChanges();
                Notification.set_flash("Đã thêm mới đơn vị vận chuyển: " + delivery.delivery_name + "", "success");
                return RedirectToAction("DeliveryIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }

            return View(delivery);
        }
        //View chỉnh sửa thông tin đơn vị vận chuyển
        public ActionResult DeliveryEdit(int? id)
        {
            var delivery = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
            if ((User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2") && (delivery.status == "1" || delivery.status == "0"))
            {
                if (delivery != null && id != null)
                {
                    return View(delivery); 
                }
                else
                {
                    Notification.set_flash("Không tồn tại! (ID = " + id + ")", "warning");
                    return RedirectToAction("DeliveryIndex");
                }
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý thông tin đơn vị vận chuyển
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult DeliveryEdit(Delivery delivery)
        {
            try
            {
                delivery.update_at = DateTime.Now;
                delivery.update_by = User.Identity.GetEmail();
                delivery.status = delivery.status;
                _db.Entry(delivery).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin đơn vị vận chuyển: "+ delivery.delivery_name + "", "success");
                return RedirectToAction("DeliveryIndex");
            }
            catch
            {
                Notification.set_flash("404!", "warning");
            }

            return View(delivery);
        }
        //Vô hiệu hóa đơn vị vận chuyển
        public ActionResult DelTrash(int? id) //bỏ sp vào thùng rác
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var delivery = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
                if (delivery == null || id == null)
                {
                    Notification.set_flash("Không tồn tại: " + delivery.delivery_name + "", "warning");
                    return RedirectToAction("DeliveryIndex");
                }
                delivery.status = "2";
                delivery.update_at = DateTime.Now;
                delivery.update_by = User.Identity.GetEmail();
                _db.Entry(delivery).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã chuyển đơn vị vận chuyển: " + delivery.delivery_name + " vào thùng rác", "success");
                return RedirectToAction("DeliveryIndex");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Khôi phục đơn vị vận chuyển
        public ActionResult Undo(int? id) // khôi phục từ thùng rác
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var delivery = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
                if (delivery == null || id == null)
                {
                    Notification.set_flash("Không tồn tại! (ID = " + id + ")", "warning");
                    return RedirectToAction("DeliveryIndex");
                }
                delivery.status = "1";
                delivery.update_at = DateTime.Now;
                delivery.update_by = User.Identity.GetEmail();
                _db.Entry(delivery).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công: " + delivery.delivery_name + "", "success");
                return RedirectToAction("DeliveryTrash");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa đơn vị vận chuyển
        public ActionResult DeliveryDelete(int? id,string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("DeliveryDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var deli = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
                if (deli == null || id == null)
                {
                    Notification.set_flash("Không tồn tại! đơn vị vận chuyển " + deli.delivery_name + "", "warning");
                    return RedirectToAction("DeliveryTrash");
                }

                return View(deli);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xác nhận xóa vĩnh viển đơn vị vận chuyển
        [HttpPost]
        [ActionName("DeliveryDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var deli = _db.Deliveries.SingleOrDefault(a => a.delivery_id == id);
                _db.Deliveries.Remove(deli);
                _db.SaveChanges();
                Notification.set_flash("Đã xoá vĩnh viễn đơn vị vận chuyển: " + deli.delivery_name + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("DeliveryTrash");
            }
            else
            {
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