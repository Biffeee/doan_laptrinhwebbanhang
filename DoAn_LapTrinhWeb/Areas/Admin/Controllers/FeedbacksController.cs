using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using DoAn_LapTrinhWeb;
using DoAn_LapTrinhWeb.Common;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.DTOs;
using DoAn_LapTrinhWeb.Model;
using PagedList;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class FeedbacksController : BaseController
    {
        private readonly DbContext db = new DbContext();
        //View list đánh giá
        public ActionResult FeedbackIndex(int?size,int?page,string search,string show)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = size ?? 10;
                var pageNumber = page ?? 1;
                ViewBag.countTrash = db.Feedbacks.Count(a => a.status == "0"); //  đếm tổng sp có trong thùng rác
                List<Feedback> reply_fb = db.Feedbacks.ToList();
                ViewBag.reply_fb = reply_fb;
                List<Feedback_Image> fb_img = db.Feedback_Image.ToList();
                ViewBag.fb_img = fb_img;
                var list = from fb in db.Feedbacks
                           join p in db.Products on fb.product_id equals p.product_id
                           join a in db.Accounts on fb.account_id equals a.account_id
                           where fb.status != "0" && fb.parent_feedback_id==0 
                           orderby fb.feedback_id descending
                           select new FeedbackDTOs
                           {
                               product_name=p.product_name,
                               product_slug=p.slug,
                               feedback_id = fb.feedback_id,
                               genre_id = p.genre_id,
                               discount_id = p.disscount_id,
                               description = fb.description,
                               rating_star = fb.rate_star,
                               status = fb.status,
                               create_at = fb.create_at,
                               User_Email = a.Email,
                               account_id = a.account_id,
                               product_id=fb.product_id,
                           };
                if (!string.IsNullOrEmpty(search))
                {
                    if (show.Equals("1"))//tìm kiếm tất cả
                        list = list.Where(s => s.feedback_id.ToString().Contains(search) || s.account_id.ToString().Contains(search));
                    else if (show.Equals("2"))//theo id
                        list = list.Where(s => s.feedback_id.ToString().Contains(search));
                    else if (show.Equals("3"))//theo tên thể loại
                        list = list.Where(s => s.account_id.ToString().Contains(search));
                    return View("FeedbackIndex", list.ToPagedList(pageNumber, 50));
                }
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //View list trash đánh giá
        public ActionResult FeedbackTrash(int? size, int? page, string search, string show)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                List<Feedback_Image> fb_img = db.Feedback_Image.ToList();
                ViewBag.fb_img = fb_img;
                var pageSize = size ?? 10;
                var pageNumber = page ?? 1;
                var list = from fb in db.Feedbacks
                           join p in db.Products on fb.product_id equals p.product_id
                           join a in db.Accounts on fb.account_id equals a.account_id
                           where fb.status == "0"
                           orderby fb.feedback_id descending
                           select new FeedbackDTOs
                           {
                               product_name = p.product_name,
                               feedback_id = fb.feedback_id,
                               genre_id = p.genre_id,
                               discount_id = p.disscount_id,
                               description = fb.description,
                               rating_star = fb.rate_star,
                               status = fb.status,
                               create_at = fb.create_at,
                               User_Email = a.Email,
                               update_at = fb.update_at,
                               account_id = a.account_id,
                           };
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //reply feedback
        public JsonResult ReplyFeedback(string feedback_content,int product_id,int genre_id,int parent_feedback_id,Feedback feedback)
        {
            bool result = false;
            try
            {
                feedback.rate_star = 5;
                feedback.description = feedback_content;
                feedback.product_id = product_id;
                feedback.genre_id = genre_id;
                feedback.parent_feedback_id = parent_feedback_id;
                feedback.status = "2";
                feedback.update_at = DateTime.Now;
                feedback.create_at = DateTime.Now;
                feedback.create_by = User.Identity.GetEmail();
                feedback.update_by = User.Identity.GetEmail();
                feedback.account_id = User.Identity.GetUserId();
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //Xác nhận duyệt đánh giá
        public ActionResult ChangeComplete(int? id, string RatingStar, string feedbackcontent,string imagefeedback)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var feedback = db.Feedbacks.SingleOrDefault(fb=>fb.feedback_id == id);
                var feedbackimage = db.Feedback_Image.Where(m => m.feedback_id == feedback.feedback_id).FirstOrDefault();
                if (feedback != null)
                {
                    feedback.status = "2";
                    feedback.update_by = User.Identity.GetName();
                    feedback.update_at = DateTime.Now;
                    db.SaveChanges();
                    Notification.set_flash("Đã xét duyệt đánh giá: " + feedback.feedback_id + "", "success");
                    if (feedback.description == null)
                    {
                        feedbackcontent = "";
                    }
                    else
                    {
                        feedbackcontent = "<div style='font-weight: 500;'>Nội dung đánh giá:</div>" +
                                           "<div>" + feedback.description + "</div>";                                          
                    }
                    string emailID = feedback.Account.Email;
                    string Feedbackid = feedback.feedback_id.ToString();
                    string productslug = feedback.Product.slug;
                    string FeebackProduct = "<tr style='border-bottom: 1px solid rgba(0,0,0,.05);' class='product_item'>" +
                                          "<td valign='middle' width ='80%' style='text-align:left; padding:0 2.5em;'> " +
                                              "<div class='product-entry'>" +                                                 
                                                  "<div class='text'>" +
                                                      "<a href='"+ Request.Url.Scheme + "://" + Request.Url.Authority + "/product/"+ feedback.Product.slug + "'> <div class='product_name'>" + feedback.Product.product_name + "</div></a>" +
                                                  "</div>" +
                                              "</div>" +
                                          "</td>" +
                                          "<td valign='middle' width='20%' style='text-align:right; padding-right: 2.5em;'>" +
                                              "<span class='price' style='color: #005f8f; font-size: 14px; font-weight: 500;'>" + feedback.Product.price.ToString("#,###₫") + "</span>" +
                                          "</td>" +
                                      "</tr>";
                    string star="";
                    if (feedback.rate_star == 1)
                    { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> "; }
                    else if (feedback.rate_star == 2)
                    { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                    else if (feedback.rate_star == 3)
                    { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                    else if (feedback.rate_star == 4)
                    { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                    else
                    { RatingStar = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'>"; }
                    RatingStar = star;
                    string FeedbackStatus = "<span style='color:#28a745;'>Đã duyệt</span>";
                    string productname = feedback.Product.product_name;
                    SendEmailFeedback(RatingStar, FeebackProduct, FeedbackStatus, emailID, Feedbackid, productname, feedbackcontent, productslug, "FeedbackAccept");
                }
                return RedirectToAction("FeedbackIndex");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //Huỷ đánh giá
        [HttpPost]
        public JsonResult CancleFeedback(int id, string RatingStar)
        {
            Boolean result;
            var feedback = db.Feedbacks.Where(m => m.feedback_id == id).FirstOrDefault();
            var feedbackimage = db.Feedback_Image.Where(m => m.feedback_id == feedback.feedback_id).FirstOrDefault();
            if (feedback.status == "2")
            {
                result = false;
            }
            else
            {
                feedback.status = "0";
                feedback.update_by = User.Identity.GetName();
                feedback.update_at = DateTime.Now;
                db.SaveChanges();
                result = true;
                string emailID = feedback.Account.Email;
                string Feedbackid = feedback.feedback_id.ToString();
                string feedbackcontent = "<div style='font-weight: 500;'>Lý do:</div>" +
                                        "<div>Vi phạm vào 1 trong các điều khoản khi đánh giá sản phẩm <a href='#' style='color: rgb(216, 1, 1); font-weight: 500 !important; text-decoration: none!important;'>(Xem tại đây)</a></div>";
                string productslug = feedback.Product.slug;
                string FeebackProduct = "<tr style='border-bottom: 1px solid rgba(0,0,0,.05);' class='product_item'>" +
                                      "<td valign='middle' width ='80%' style='text-align:left; padding:0 2.5em;'> " +
                                          "<div class='product-entry'>" +                                             
                                              "<div class='text'>" +
                                                  "<a href='"+Request.Url.Scheme + "://" + Request.Url.Authority + "/product/" + feedback.Product.slug + "'> <div class='product_name'>" + feedback.Product.product_name + "</div></a>" +
                                              "</div>" +
                                          "</div>" +
                                      "</td>" +
                                      "<td valign='middle' width='20%' style='text-align:right; padding-right: 2.5em;'>" +
                                          "<span class='price' style='color: #005f8f; font-size: 14px; font-weight: 500;'>" + feedback.Product.price.ToString("#,###₫") + "</span>" +
                                      "</td>" +
                                  "</tr>";
                string star = "";
                if (feedback.rate_star == 1)
                { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> "; }
                else if (feedback.rate_star == 2)
                { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                else if (feedback.rate_star == 3)
                { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                else if (feedback.rate_star == 4)
                { star = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620663/star-o_yuph59.png'>"; }
                else
                { RatingStar = "<img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'> <img src='https://res.cloudinary.com/van-nam/image/upload/v1635620736/star_zxwzal.png'>"; }
                RatingStar = star;
                string FeedbackStatus = "<span style='color:#dc3545;'>Bị huỷ</span>";
                string productname = feedback.Product.product_name;
                SendEmailFeedback( RatingStar,  FeebackProduct,  FeedbackStatus,  emailID, Feedbackid, productname,  feedbackcontent,  productslug, "FeedbackCancled");
                return Json(new { result, Message = "Huỷ đánh giá ID'" + feedback.feedback_id + "' thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //send mail khi thay đổi trạng thái
        public void SendEmailFeedback(string RatingStar, string FeedbackProduct, string FeedbackStatus, string emailID,string Feedbackid, string productname,  string feedbackcontent, string productslug, string emailFor)
        {            
            var fromEmail = new MailAddress(AccountEmail.UserEmail, AccountEmail.Name);
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = AccountEmail.Password;
            string subject = "";
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "MailFeedback" + ".cshtml"); 
            if (emailFor == "FeedbackAccept")
            {
                subject = "Đánh giá của bạn về sản phẩm '"+ productname + "' Đã được duyệt";
                body = body.Replace("{{FeedbackRatingStar}}", RatingStar);
                body = body.Replace("{{ProductFeedback}}", FeedbackProduct);
                body = body.Replace("{{Feedbackcontent}}", feedbackcontent);
                body = body.Replace("{{FeedbackStatus}}", FeedbackStatus);
                body = body.Replace("{{ButtonConfirmLink}}", Request.Url.Scheme + "://" + Request.Url.Authority + "/product/" + productslug); 
                body = body.Replace("{{ButtonConfirmName}}", "Xem đánh giá");
            }
            else if (emailFor == "FeedbackCancled")
            {
                subject = "Đánh giá của bạn về sản phẩm '" + productname + "' Đã bị huỷ";
                body = body.Replace("{{FeedbackRatingStar}}", RatingStar);
                body = body.Replace("{{ProductFeedback}}", FeedbackProduct);
                body = body.Replace("{{Feedbackcontent}}", feedbackcontent);
                body = body.Replace("{{FeedbackStatus}}", FeedbackStatus);
                body = body.Replace("{{ButtonConfirmLink}}",  Request.Url.Scheme + "://" + Request.Url.Authority +"/product/" + productslug);
                body = body.Replace("{{ButtonConfirmName}}", "Xem sản phẩm");
            }
            var smtp = new SmtpClient
            {
                Host = AccountEmail.Host, 
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
