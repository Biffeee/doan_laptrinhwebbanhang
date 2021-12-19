using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class CommentReplyController : BaseController
    {
        // GET: Admin/CommentReply
        readonly DbContext db = new DbContext();
        public ActionResult CmtIndex(int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                ViewBag.rep_comment = db.Reply_Comments.ToList();
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in db.NewsComments
                           where (a.status != "0")
                           orderby a.create_at descending
                           select a;
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //danh sách reply comment
        public ActionResult ReplyIndex(int? size, int? page)
        {
            if (User.Identity.GetRole() == "0" || User.Identity.GetRole() == "2")
            {
                var pageSize = (size ?? 10);
                var pageNumber = (page ?? 1);
                var list = from a in db.Reply_Comments
                           orderby a.create_at descending
                           select a;
                return View(list.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        //trả lời bình luận
        public async Task <ActionResult> ReplyComment(int comment_id,string reply_comment_content,Reply_Comments reply_Comments)
        {
            bool result = false;
            try
            {
                reply_Comments.comment_id = comment_id;
                reply_Comments.reply_comment_content = reply_comment_content;
                reply_Comments.account_id = User.Identity.GetUserId();
                reply_Comments.status = "2";
                reply_Comments.create_at = DateTime.Now;
                db.Reply_Comments.Add(reply_Comments);
                var comment = db.NewsComments.FirstOrDefault(m => m.comment_id == comment_id);
                comment.status = "2";
                db.Entry(comment).State = EntityState.Modified;
                await db.SaveChangesAsync();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        //duyệt tất cả bình luận
        public JsonResult ApprovedAllComment()
        {
            bool result = false;
            try
            {
                List<NewsComments> comment = db.NewsComments.ToList();
                foreach(var item in comment)
                {
                    if (item.status == "1")
                    {
                        item.status = "2";
                        db.SaveChanges();
                        result = true;
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        //duyệt từng bình luận
        [HttpPost]
        public JsonResult AcceptComment(int comment_id)
        {
            bool result = false;
            try
            {
                var comment = db.NewsComments.FirstOrDefault(m=>m.comment_id== comment_id && m.status=="1");
                if (comment != null)
                {
                    comment.status = "2";
                    db.SaveChanges();
                    result = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //xóa bình luận
        [HttpPost]
        public JsonResult DeleteComment(int del_comment_id)
        {
            bool result = false;
            try
            {
                var comment = db.NewsComments.FirstOrDefault(m => m.comment_id == del_comment_id && m.status == "1");
                if (comment != null)
                {
                    db.NewsComments.Remove(comment);
                    db.SaveChanges();
                    result = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //duyệt tất cả reply comment
        public JsonResult ApprovedAllReplyComment()
        {
            bool result = false;
            try
            {
                List<Reply_Comments> repcomment = db.Reply_Comments.ToList();
                foreach (var item in repcomment)
                {
                    if (item.status == "1")
                    {
                        item.status = "2";
                        db.SaveChanges();
                        result = true;
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        //duyệt từng reply comment
        [HttpPost]
        public JsonResult AcceptReplyComment(int rep_comment_id)
        {
            bool result = false;
            try
            {
                var repcomment = db.Reply_Comments.FirstOrDefault(m => m.reply_comment_id == rep_comment_id && m.status == "1");
                if (repcomment != null)
                {
                    repcomment.status = "2";
                    db.SaveChanges();
                    result = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //xóa reply comment
        [HttpPost]
        public JsonResult DeleteRepComment(int del_rep_comment_id)
        {
            bool result = false;
            try
            {
                var repcomment = db.Reply_Comments.FirstOrDefault(m => m.reply_comment_id == del_rep_comment_id && m.status == "1");
                if (repcomment != null)
                {
                    db.Reply_Comments.Remove(repcomment);
                    db.SaveChanges();
                    result = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}