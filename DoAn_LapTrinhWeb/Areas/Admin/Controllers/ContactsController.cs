using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Hosting;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.Common;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Models;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class ContactsController : BaseController
    {
        private readonly DbContext _db = new DbContext();
        //View list liên lạc khách hàng
        public ActionResult ContactIndex(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                ViewBag.countTrash = _db.Contacts.Count(a => a.status == "2");
                var list = from a in _db.Contacts
                           where (a.status == "1" || a.status == "0")
                           orderby a.contact_id descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.contact_id.ToString().Contains(search) || s.name.Contains(search) || s.email.Contains(search) || s.phone.ToString().Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.contact_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên 
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.name.Contains(search));
                    else if (show.Equals("4"))//theo email
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.email.Contains(search));
                    else if (show.Equals("5"))//theo phone
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.phone.ToString().Contains(search));
                    return View("ContactIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View list trash liên hệ khách hàng
        public ActionResult ContactTrash(string search, string show, int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in _db.Contacts
                           where a.status == "2"
                           orderby a.update_at descending
                           select a;
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.contact_id.ToString().Contains(search) || s.name.Contains(search) || s.email.Contains(search) || s.phone.ToString().Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.contact_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.name.Contains(search));
                    else if (show.Equals("4"))//theo email
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.email.Contains(search));
                    else if (show.Equals("5"))//theo phone
                        list = (IOrderedQueryable<Contact>)list.Where(s => s.phone.ToString().Contains(search));
                    return View("ContactTrash", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View trả lời khách hàng
        public ActionResult Reply(int? id)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var contact = _db.Contacts.SingleOrDefault(a => a.contact_id == id);
                if (contact == null || id == null)
                {
                    Notification.set_flash("Không tồn tại liên hệ từ khách hàng!", "danger");
                    return RedirectToAction("ContactIndex");
                }
                return View(contact);
            }
            else
            {
                //nếu không phải là admin hoặc biên tập viên thì sẽ back về trang chủ bảng điều khiển
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Code xử lý trả lời liên hệ khách hàng
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(Contact contact,string Contact_id, string UserEmail,string Content,string Reply,string Update_by,string Roles )
        {
            try
            {
                contact.reply = contact.reply;
                contact.content = contact.content;
                contact.flag = 1;
                contact.update_at = DateTime.Now;
                contact.update_by = User.Identity.GetEmail();
                Contact_id = contact.contact_id.ToString();
                UserEmail = contact.email;
                Content = contact.content;
                Reply = contact.reply;
                Update_by= User.Identity.GetName();
                if (User.Identity.GetRole() == "0")
                {
                    Roles = "Quản trị viên";
                }
                else
                {
                    Roles = "Biên tập viên";
                }

                SendEmailReply(Contact_id, UserEmail, Content, Reply , Update_by,Roles);
                Notification.set_flash("Đã trả lời liên hệ!", "success");
                _db.Entry(contact).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("ContactIndex");
            }
            catch
            {
                Notification.set_flash("Lỗi", "danger");
            }
            return View(contact);
        }
        //Gửi mail sau khi trả lời câu hỏi khách hàng
        public void SendEmailReply(string Contact_id, string UserEmail, string Content, string Reply, string Update_by, string Roles)
        {
           
            var fromEmail = new MailAddress(AccountEmail.UserEmailSupport, AccountEmail.Name); 
            var toEmail = new MailAddress(UserEmail);
            //nhập password của bạn
            var fromEmailPassword = AccountEmail.Password;
            string subject = "";
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "MailContact" + ".cshtml"); 
            subject = "Quý khách có câu trả lời từ SPM SUPERMARKET [LH" + Contact_id + "]";
            body = body.Replace("{{ViewBag.contact_id}}", "LH"+Contact_id);
            body = body.Replace("{{ViewBag.contact_content}}", Content);
            body = body.Replace("{{ViewBag.contact_reply}}", Reply);
            body = body.Replace("{{ViewBag.Name}}", Update_by);
            body = body.Replace("{{ViewBag.Roles}}", Roles);
            var smtp = new SmtpClient
            {
                Host = AccountEmail.Host, 
                Port = 587,
                EnableSsl = true, 
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
        public ActionResult DelTrash(int? id) 
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var contact = _db.Contacts.SingleOrDefault(a => a.contact_id == id);
                if (contact == null || id == null)
                {
                    Notification.set_flash("Không tồn tại liên hệ: " + "LH" + id + "", "warning");
                    return RedirectToAction("Index");
                }
                contact.status = "2";
                contact.update_at = DateTime.Now;
                contact.update_by = User.Identity.GetEmail();
                _db.Entry(contact).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Đã chuyển liên hệ: " + "LH" + id + " vào thùng rác!", "success");
                return RedirectToAction("ContactIndex");
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
                var contact = _db.Contacts.SingleOrDefault(a => a.contact_id == id);
                if (contact == null || id == null)
                {
                    Notification.set_flash("Không tồn tại liên hệ: " + "LH"+id+ "", "warning");
                    return RedirectToAction("Index");
                }
                contact.status = "1";
                contact.update_at = DateTime.Now;
                contact.update_by = User.Identity.GetEmail();
                _db.Entry(contact).State = EntityState.Modified;
                _db.SaveChanges();
                Notification.set_flash("Khôi phục thành công liên hệ: " + "LH" + id + "", "success");
                return RedirectToAction("ContactTrash");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Xóa liên lạc
        public ActionResult ContactDelete(int? id, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("ContactDelete", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var contact = _db.Contacts.SingleOrDefault(a => a.contact_id == id);
                if (contact == null)
                {
                    Notification.set_flash("Không tồn tại liên hệ: "+"LH"+ contact.contact_id + "", "warning");
                    return RedirectToAction("ContactTrash");
                }
                return View(contact);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        [HttpPost]
        [ActionName("ContactDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var contact = _db.Contacts.SingleOrDefault(a => a.contact_id == id);
                _db.Contacts.Remove(contact);
                _db.SaveChanges();
                Notification.set_flash("Đã xoá vĩnh viễn liên hệ: " + "LH" + id + "", "success");
                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("ContactTrash");
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