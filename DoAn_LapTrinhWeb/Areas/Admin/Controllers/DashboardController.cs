using System;
using System.Linq;
using System.Web.Mvc;
using DoAn_LapTrinhWeb.DTOs;
using DoAn_LapTrinhWeb.Model;

namespace DoAn_LapTrinhWeb.Areas.Admin.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly DbContext db = new DbContext();
        //View thống kê
        public ActionResult Index(string sortOrder, DateTime? picker1, DateTime? picker2)
        {
            DateTime today = DateTime.Today;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ResetSort = String.IsNullOrEmpty(sortOrder) ? "" : "";
            ViewBag.TodaySortParm = sortOrder == "today" ? "today" : "today";
            ViewBag.DaysAgoSortParm = sortOrder == "7daysago" ? "7daysago" : "7daysago";
            ViewBag.MonthAgoSortParm = sortOrder == "month_ago" ? "month_ago" : "month_ago";
            ViewBag.MonthSortParm = sortOrder == "month" ? "month" : "month";
            ViewBag.YearSortParm = sortOrder == "year" ? "year" : "year";
            switch (sortOrder)
            {
                case "today":
                    ViewBag.SortBy = "Thống kê theo: Hôm nay";
                    ViewBag.DateSort = "Ngày "+DateTime.Now.Day;
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1"&& m.create_at.Day == today.Day && m.create_at.Month == today.Month).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => m.create_at.Day == today.Day && m.status != "2" && m.create_at.Month == today.Month).Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && m.create_at.Day == today.Day && m.create_at.Month == today.Month).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2" && m.parent_feedback_id == 0 && m.create_at.Day == today.Day && m.create_at.Month == today.Month).Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && m.create_at.Day == today.Day && m.create_at.Month == today.Month).Count();
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "0" && m.oder_date.Day == today.Day && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && m.oder_date.Day == today.Day && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && m.oder_date.Day == today.Day && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && m.oder_date.Day == today.Day && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date.Day == today.Day && g.Key.b.create_at.Month == today.Month
                                                orderby g.Key.b.create_at descending
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
                case "7daysago":
                    ViewBag.SortBy = "Thống kê theo: 7 ngày qua";
                    //tính 7 ngày trước, trừ ra ngày hiện tại nên addday(-8) 
                    DateTime daysago = DateTime.Now.AddDays(-8);
                    DateTime end_date = DateTime.Now.AddDays(-1);
                    ViewBag.DateSort = DateTime.Now.AddDays(-7).ToString("dd/MM") + " - " + DateTime.Now.AddDays(-1).ToString("dd/MM");
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1" && (m.create_at > daysago && m.create_at < end_date)).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => (m.create_at > daysago && m.create_at < end_date) && m.status != "2").Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && (m.create_at > daysago && m.create_at < end_date)).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2").Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && (m.create_at > daysago && m.create_at < DateTime.Now)).Count();
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "0" && (m.oder_date > daysago && m.oder_date < DateTime.Now)).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && (m.oder_date > daysago && m.oder_date < DateTime.Now)).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && (m.oder_date > daysago && m.oder_date < DateTime.Now)).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && (m.oder_date > daysago && m.oder_date < DateTime.Now)).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date.Day == today.Day && g.Key.b.oder_date.Month == today.Month
                                                orderby g.Key.b.create_at descending
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
                case "month_ago":
                    ViewBag.SortBy = "Thống kê theo: Tháng trước";
                    DateTime month_ago = DateTime.Now.AddMonths(-1);
                    ViewBag.DateSort = "Tháng " + month_ago.Month;
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1" && m.create_at.Month == month_ago.Month).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => m.create_at.Month == month_ago.Month && m.status != "2").Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && m.create_at.Month == month_ago.Month).Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && m.create_at.Month == month_ago.Month).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2" && m.parent_feedback_id == 0 && m.create_at.Month == month_ago.Month).Count();
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "0" && m.oder_date.Month == month_ago.Month).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && m.oder_date.Month == month_ago.Month).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && m.oder_date.Month == month_ago.Month).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && m.oder_date.Month == month_ago.Month).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date == month_ago
                                                orderby g.Key.b.create_at descending
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
                case "month":
                    ViewBag.SortBy = "Thống kê theo: Tháng này";
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1" && m.create_at.Month == today.Month).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => m.create_at.Month == today.Month && m.status != "2").Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && m.create_at.Month == today.Month).Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && m.create_at.Month == today.Month).Count();
                    ViewBag.DateSort = "Tháng " + DateTime.Now.Month;
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "0" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2" && m.parent_feedback_id == 0 && m.create_at.Month == today.Month).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date.Month == today.Month
                                                orderby g.Key.b.create_at descending
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
                case "year":
                    ViewBag.SortBy = "Thống kê theo: Năm nay";
                    ViewBag.DateSort = "Năm " + DateTime.Now.Year;
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1"&& m.create_at.Year == today.Year).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => m.create_at.Year == today.Year && m.status != "2").Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && m.create_at.Year == today.Year).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2" && m.parent_feedback_id == 0 && m.create_at.Year == today.Year).Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && m.create_at.Year == today.Year).Count();
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "0" && m.oder_date.Year == today.Year).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && m.oder_date.Year == today.Year).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && m.oder_date.Year == today.Year).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && m.oder_date.Year == today.Year).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date.Year == today.Year
                                                orderby g.Key.b.create_at descending
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
                default:
                    ViewBag.DateSort = "Thống kê theo: Tháng";
                    ViewBag.DateSort = "Tháng " + DateTime.Now.Month;
                    ViewBag.CountProducts = db.Products.Where(m => m.status == "1"&& m.create_at.Month == today.Month).Count();
                    ViewBag.CountContact = db.Contacts.Where(m => m.create_at.Month == today.Month && m.status != "2").Count();
                    ViewBag.CountPost = db.News.Where(m => m.status == "1" && m.create_at.Month == today.Month).Count();
                    ViewBag.CountFeedback = db.Feedbacks.Where(m => m.status == "2" && m.parent_feedback_id==0 && m.create_at.Month == today.Month).Count();
                    ViewBag.CountUsers = db.Accounts.Where(m => m.status == "1" && m.Role == "1" && m.create_at.Month == today.Month).Count();
                    ViewBag.CountOrderCancled = db.Orders.Where(m => m.status == "1" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderWaitting = db.Orders.Where(m => m.status == "1" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderProcessing = db.Orders.Where(m => m.status == "2" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountOrderComplete = db.Orders.Where(m => m.status == "3" && m.oder_date.Month == today.Month).Count();
                    ViewBag.CountProductSale = (from a in db.Order_Detail
                                                join b in db.Orders on a.order_id equals b.order_id
                                                join c in db.Products on a.product_id equals c.product_id
                                                group a by new { a.order_id, b } into g
                                                where g.Key.b.status == "3" && g.Key.b.oder_date.Month == today.Month
                                                orderby g.Key.b.create_at descending 
                                                select new OrderDTOs
                                                {
                                                    total_quantity = g.Sum(m => m.quantity),
                                                    order_id = g.Key.order_id,
                                                    status = g.Key.b.status,
                                                    create_at = g.Key.b.create_at
                                                }).Sum(m => (int?)m.total_quantity) ?? 0;
                    break;
            }
            ViewBag.CountTurnover = db.Orders.Where(m => m.status == "3" && m.oder_date.Month == today.Month).Sum(x => (int?)x.total) ?? 0;
            //Tổng doanh thu năm
            ViewBag.CountTurnover1 = db.Orders.Where(m => m.status == "3" && m.oder_date.Year == today.Year).Sum(x => (int?)x.total) ?? 0;
            ViewBag.ProcessingOrder = "processing";
            ViewBag.CompleteOrder = "complete";
            ViewBag.WaitingOrder = "waiting";
            return View();
        }
        //Biểu đồ top 10 sản phẩm bán chạy
        public ActionResult Getbarcharts()
        {
            DateTime today = DateTime.Today;
            var query = db.Order_Detail.Include("Product")
                   .Where(m=>m.Order.oder_date.Month==today.Month && m.Order.status=="3")
                   .GroupBy(p => p.Product.product_name)
                   .Select(g => new { name = g.Key, count = g.Sum(w => w.quantity)}).OrderByDescending(m=>m.count).Take(10).ToList();
            return Json(query, JsonRequestBehavior.AllowGet);
        }
    }
}