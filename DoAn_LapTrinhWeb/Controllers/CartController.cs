using DoAn_LapTrinhWeb.Common;
using DoAn_LapTrinhWeb.Common.Helpers;
using DoAn_LapTrinhWeb.DTOs;
using DoAn_LapTrinhWeb.Model;
using DoAn_LapTrinhWeb.Models;
using DoAn_LapTrinhWeb.PaymentLibrary;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
namespace DoAn_LapTrinhWeb.Controllers
{
    public class CartController : Controller
    {        
        private DbContext db = new DbContext();
        //View Giỏ hàng
        public ActionResult ViewCart()
        {
            var cart = this.GetCart();
            ViewBag.Quans = cart.Item2;
            var listProduct = cart.Item1.ToList();
            double discount =0d;
            if (Session["Discount"] != null && Session["Discountcode"] != null)
            {
                var code = Session["Discountcode"].ToString();
                var discountupdatequan = db.Discounts.Where(d => d.discounts_code == code).FirstOrDefault();

                if (discountupdatequan.quantity == "0")
                {
                    Notification.set_flash("Mã giảm giá đã hết lượt sử dụng", "danger");
                }
                else if (discountupdatequan.discount_start >= DateTime.Now || discountupdatequan.discount_end <= DateTime.Now)
                {
                    Notification.set_flash("Mã giảm giá không thể sử dụng trong thời gian này", "danger");
                }
                else
                {
                    discount = Convert.ToDouble(Session["Discount"].ToString());
                }
                Session.Remove("Discount");
                Session.Remove("Discountcode");
            }
            BannerGlobal();
            ViewBag.Discount = discount;
            return View(listProduct);
        }
        //Xóa sản phẩm khỏi giỏ hàng
        public ActionResult RemoveProduct(int id)
        {
            List<Cart> lst = Session["Cart"] as List<Cart>;
            Cart item = lst.Find(n => n.Product_ID == id);
            lst.Remove(item);
            return RedirectToAction("ViewCart");
        }
        //Cập nhật giỏ hàng
        [HttpPost]
        public ActionResult UpdateCart(int id, FormCollection frm)
        {
            List<Cart> lst = Session["Cart"] as List<Cart>;
            Cart item = lst.Find(n => n.Product_ID == id);
            int soluong = int.Parse(frm["txtSoluong"].ToString());
            item.Quantity = soluong;//cap nhat so luong moi
            return RedirectToAction("ViewCart");
        }
        [Authorize]
        public ActionResult Checkout(ChekcoutOrderDTO chekcout)
        {
            int userId = User.Identity.GetUserId();
            ViewBag.ProvincesList = db.Provinces.OrderBy(m => m.province_name).ToList();
            ViewBag.DistrictsList = db.Districts.OrderBy(m => m.type).ThenBy(m => m.district_name).ToList();
            ViewBag.WardsList = db.Wards.OrderBy(m => m.type).ThenBy(m => m.ward_name).ToList();
            var listaddress = db.Account_Address.Where(m=>m.account_id==userId).OrderByDescending(m=>m.account_address_default).ToList();
            ViewBag.CountAddress = db.Account_Address.Where(m => m.account_id == userId).Count();
            if (listaddress != null)
            {
                ViewBag.ListAdress = listaddress;
            }
            var user = db.Account_Address.SingleOrDefault(u => u.account_id == userId && u.account_address_default==true);
            if (user != null)
            {
                chekcout.checkout_username = user.account_address_username;
                chekcout.checkout_phone_number = user.account_address_phonenumber;
                chekcout.checkout_address=user.account_address_content +", "+ user.Wards.type +" "+ user.Wards.ward_name+", "+ user.Districts.type + " " + user.Districts.district_name +", " + user.Provinces.province_name;
             }
            var cart = this.GetCart();
            if (cart.Item2.Count < 1)
            {
                Notification.set_flash("Bạn chưa có sản phẩm trong giỏ hàng", "Warning");
                return RedirectToAction(nameof(ViewCart));
            }
            var products = cart.Item1;
            double total = 0d;
            double discount = 0d;
            double productPrice = 0d;
            for (int i = 0; i < products.Count; i++)
            {
                var item = products[i];
                productPrice = item.price;
                if (item.Discount != null)
                {
                    if (item.Discount.discount_start < DateTime.Now && item.Discount.discount_end > DateTime.Now)
                    {
                        productPrice = item.price - item.Discount.discount_price;
                    }
                }
                total += productPrice * cart.Item2[i];
            }
            //Áp dụng mã giảm giá
            if (Session["Discount"] != null && Session["Discountcode"] != null)
            {
                var code = Session["Discountcode"].ToString();
                var userid = User.Identity.GetUserId();
                var oderdetail = db.Order_Detail.FirstOrDefault(m => m.Order.account_id == userid);
                if (oderdetail!=null && code == oderdetail.discount_code)
                {
                    Notification.set_flash("Bạn đã sử dụng mã giảm giá này", "danger");
                    Session.Remove("Discount");
                    Session.Remove("Discountcode");
                }
                else
                {
                    discount = Convert.ToDouble(Session["Discount"].ToString());
                }
            }
            ViewBag.Total = total;//truyền tạm tính lên view
            ViewBag.Discount = discount;//truyền discount lên view
            BannerGlobal();
            return View(chekcout);
        }
        //thay đổi địa chỉ nhận hàng
        public ActionResult ChangeAdressOrder(Account_Address model)
        {
            bool result;
            if (User.Identity.IsAuthenticated)
            {
                var address = db.Account_Address.FirstOrDefault(m => m.account_address_id == model.account_address_id);
                var checkdefault = db.Account_Address.ToList();
                var userid = User.Identity.GetUserId();
                foreach (var item in checkdefault)
                {
                    if (item.account_address_default == true && item.account_id==userid)
                    {
                        item.account_address_default = false;
                        db.SaveChanges();
                    }
                }
                address.account_address_default = true;
                db.SaveChanges();
                result = true;
            }
            else
            {
                result = false;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //Lưu giỏ hàng
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SaveOrder(ChekcoutOrderDTO chekcout, OrderAddress orderAddress, string check_discount, string SubjectMail, string ButtonConfirmlink,string ButtonConfirm,string OrderPayment, string OrderDelivery,string DeliveryId, string SubOrderTotal,string Discount_Price,string ProductOrder, string OrderId,string OrderStatus, string OrderTotal, string UserEmail, string UserName, string UserPhoneNumber, string UserAddress)
        {
            try
            {
                if (Session["Discount"] != null && Session["Discountcode"] != null)
                {
                    check_discount = Session["Discountcode"].ToString();
                    var discountupdatequan = db.Discounts.Where(d => d.discounts_code == check_discount).FirstOrDefault();
                    if (discountupdatequan.quantity == "0" || discountupdatequan.discount_start >= DateTime.Now || discountupdatequan.discount_end <= DateTime.Now)
                    {
                        return RedirectToAction("ViewCart", "Cart");
                    }
                    else
                    {
                        int newquantity = (Int32.Parse(discountupdatequan.quantity) - 1);
                        discountupdatequan.quantity = newquantity.ToString();
                        if (discountupdatequan.quantity == "0")
                        {
                            discountupdatequan.status = "0";
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    check_discount = null;
                }
                //lưu thông tin địa chỉ khách hàng
                if (chekcout.checkout_username != null && chekcout.checkout_username != null && chekcout.checkout_address != null && chekcout.checkout_phone_number != null)
                {
                    orderAddress.order_address_username = chekcout.checkout_username;
                    orderAddress.order_address_content = chekcout.checkout_address;
                    orderAddress.order_address_phonenumber = chekcout.checkout_phone_number;
                    orderAddress.order_adress_email = User.Identity.GetEmail();
                    db.OrderAddress.Add(orderAddress);
                }
                else
                {
                    Notification.set_flash("Vui lòng cập nhật thông tin nhận hàng", "warning");
                    return RedirectToAction("Checkout", "Cart");
                }
                //get sản phẩm
                var cart = this.GetCart();
                //format giá tiền
                CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                //tính tổng giá để gửi email
                double pricesum = 0;
                double dcprice1 = Convert.ToDouble(Session["Discount"]);//tính giảm giá
                string productquancheck = "0";               
                string payment_status;
                if (chekcout.payment_id == 1)
                {
                    payment_status = "2";
                }
                else
                {
                    payment_status = "1";
                }
                //tạo danh sách sản phẩm để cập nhập lại số lượng sản phẩm khi khách đặt hàng xong
                var listProduct = new List<Product>();
                var order = new Order()
                {
                    account_id = User.Identity.GetUserId(),
                    create_at = DateTime.Now,
                    create_by = User.Identity.GetUserId().ToString(),
                    status = "1",
                    order_note = Request.Form["OrderNote"].ToString(),
                    delivery_id = 1,
                    oder_date = DateTime.Now,
                    update_at = DateTime.Now,
                    payment_transaction = payment_status,
                    order_address_id = orderAddress.order_address_id,
                    payment_id = chekcout.payment_id,
                    update_by = User.Identity.GetUserId().ToString(),
                    total = Convert.ToDouble(TempData["Total"])
                };
                //lưu sản phẩm vào order detail: id, giá(sau cùng), số lượng
                for (int i = 0; i < cart.Item1.Count; i++)
                {
                    var item = cart.Item1[i];
                    var _price = item.price;
                    if (item.Discount != null)
                    {
                        if (item.Discount.discount_start < DateTime.Now && item.Discount.discount_end > DateTime.Now)
                        {
                            _price = item.price - item.Discount.discount_price;
                        }
                    }
                    //thêm sản phẩm vào order detail
                    order.Order_Detail.Add(new Order_Detail
                    {
                        create_at = DateTime.Now,
                        create_by = User.Identity.GetUserId().ToString(),
                        genre_id = item.genre_id,
                        price = _price,
                        product_id = item.product_id,
                        discount_code = check_discount,
                        quantity = cart.Item2[i],
                        status = "1",
                        update_at = DateTime.Now,
                        update_by = User.Identity.GetUserId().ToString()
                    }) ;
                    // Xóa cart
                    Response.Cookies["product_" + item.product_id].Expires = DateTime.Now.AddDays(-10);
                    // Thay đổi số lượng và số lượt mua của product 
                    var product = db.Products.SingleOrDefault(p => p.product_id == item.product_id);
                    productquancheck = product.quantity;
                    product.buyturn += cart.Item2[i];
                    product.quantity = (Convert.ToInt32(product.quantity ?? "0") - cart.Item2[i]).ToString();
                    double price_product_email = product.price;
                    if(product.Discount.discount_start < DateTime.Now && product.Discount.discount_end > DateTime.Now && product.Discount.status == "1")
                    {
                        price_product_email = product.price - product.Discount.discount_price;
                    }
                    listProduct.Add(product);
                    //tiền tạm tính khi gửi email
                    pricesum += (_price * cart.Item2[i]);
                    //tạo danh sách sản phẩm khi gửi email: ảnh, tên,số lượng và giá sản phẩm
                    ProductOrder += "<tr style='border-bottom: 1px solid rgba(0,0,0,.05);' class='product_item'>"+
                                        "<td valign='middle' width ='80%' style='text-align:left; padding:0 2.5em;'> " +
                                            "<div class='product-entry'>" +                                             
                                                "<div class='text'>" +
                                                    "<a href='"+ Request.Url.Scheme + "://" + Request.Url.Authority +"/product/"+item.slug+"'> <div class='product_name'>" + item.product_name + "</div></a>" +
                                                    "<span class='product_quantity'>Số lượng: " + cart.Item2[i] + "</span>" +
                                                "</div>" +
                                            "</div>" +
                                        "</td>" +
                                        "<td valign='middle' width='20%' style='text-align:right; padding-right: 2.5em;'>" +
                                            "<span class='price' style='color: #005f8f; font-size: 14px; font-weight: 500;'>" + price_product_email.ToString("#,###", cul.NumberFormat)+"₫" + "</span>" +
                                        "</td>"+
                                    "</tr>";
                    if (Session["Discount"] == null)//nếu không có giảm giá thì discoutprice = 0 và ngược lại
                    { Discount_Price = "0₫"; }
                    else
                    {
                        if (dcprice1 <= 100)
                        {
                            Discount_Price = "<span style='color: #28a745'>"+((dcprice1 * pricesum) / 100).ToString("-#,0", cul.NumberFormat)+"₫"+ "</span>";
                        }
                        else
                        {
                            Discount_Price = "<span style='color: #28a745'>"+dcprice1.ToString("-#,0", cul.NumberFormat)+"₫" + "</span>";
                        }
                    }
                }
                //thêm dữ liệu vào table
                if (productquancheck.Trim() != "0")
                {
                    db.Orders.Add(order);
                }
                else
                {
                    Notification.set_flash("Sản phẩm đã hết hàng", "danger");
                    return RedirectToAction("ViewCart", "Cart");
                }
                await db.SaveChangesAsync();//chờ các bảng thêm xong thì lưu lại
                var delivery = db.Deliveries.FirstOrDefault(m => m.delivery_id == order.delivery_id);
                var payment = db.Payments.FirstOrDefault(m => m.payment_id == order.payment_id);
                OrderId = order.order_id.ToString();
                if (order.total == 0)//tổng số tiền khi gửi email
                { 
                    OrderTotal = "0₫";
                }
                else
                { 
                    OrderTotal = order.total.ToString("#,###", cul.NumberFormat)+"₫"; 
                }
                SubOrderTotal = pricesum.ToString("#,###", cul.NumberFormat)+"₫";
                UserEmail = User.Identity.GetEmail();
                UserName = orderAddress.order_address_username;
                UserPhoneNumber = orderAddress.order_address_phonenumber;
                UserAddress = orderAddress.order_address_content;
                OrderStatus = "<span style='color:#ffc107'>Chờ xử lý</span>";//tạo trạng thái cho đơn hàng
                OrderDelivery = delivery.delivery_name;
                DeliveryId =OrderId ;
                OrderPayment = payment.payment_name;
                ButtonConfirm = "Quản lý đơn hàng";
                ButtonConfirmlink = "/account/order_detail/" + order.order_id;//đường link đầy đủ sẽ là
                SubjectMail = "Đặt hàng thành công";
                //thay đổi số lượt mã giảm giá
                //tạo temp để truyển id sang action vnpay để tiến hành thanh toánq đơn hàng
                TempData["OrderId"] = order.order_id;
                TempData["AddOrderSuccess"] = true;
                TempData["Pament_Transaction"] = payment_status;
                switch (chekcout.payment_id)
                {                  
                    case 2://thanh toán bằng vnpay
                        return RedirectToAction("vnpay", "PaymentMethods");                  
                    case 6://thanh toán bằng paypal
                        return RedirectToAction("PaymentWithPaypal", "PaymentMethods");
                    default:
                        Notification.set_flash("Đặt hàng thành công", "success");
                        SendVerificationLinkEmail(SubjectMail, ButtonConfirmlink, ButtonConfirm, OrderPayment, OrderDelivery,DeliveryId, SubOrderTotal, Discount_Price, UserEmail, UserName, UserPhoneNumber, UserAddress, OrderId, OrderStatus, OrderTotal, ProductOrder);
                        //xoá discount sau khi đặt hàng thành công
                        Session.Remove("Discount");
                        Session.Remove("Discountcode");
                        return RedirectToAction("TrackingOrder", "Account");
                }
            }
            catch
            {
                TempData["AddOrderSuccess"] = false;
            }
            return RedirectToAction("TrackingOrder", "Account");
        }        
        [NonAction]
        public void SendVerificationLinkEmail(string SubjectMail, string ButtonConfirmlink, string ButtonConfirm,string OrderPayment, string OrderDelivery, string DeliveryId, string SubOrderTotal, string Discount_Price, string UserEmail,string UserName,string UserPhoneNumber, string UserAddress,string OrderId,string OrderStatus, string OrderTotal,string ProductOrder)
        {      
            var fromEmail = new MailAddress(AccountEmail.UserEmailSupport, AccountEmail.Name); 
            var toEmail = new MailAddress(UserEmail);         
            var fromEmailPassword = AccountEmail.Password;
            string subject;
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "MailOrders" + ".cshtml"); 
                subject = SubjectMail;
                body = body.Replace("{{OrderId}}", OrderId);
                body = body.Replace("{{BodyContent}}", "Yêu cầu đặt hàng cho đơn hàng <span style='color: rgb(1, 181, 187); font-weight: 500;'>#"+OrderId+"</span> của bạn đã được tiếp nhận và đang chờ Nhà bán hàng xử lý, với hình thức thanh toán là <span>"+OrderPayment+"</span> Chúng tôi sẽ tiếp tục cập nhật với bạn về trạng thái tiếp theo của đơn hàng.");
                body = body.Replace("{{OrderStatus}}", OrderStatus);
                body = body.Replace("{{ButtonConfirm}}", ButtonConfirm);
                body = body.Replace("{{ButtonConfirmLink}}", Request.Url.Scheme + "://" + Request.Url.Authority + ButtonConfirmlink);
                body = body.Replace("{{UserEmail}}", UserEmail);
                body = body.Replace("{{DiscountPrice}}", Discount_Price);
                body = body.Replace("{{UserName}}", UserName);
                body = body.Replace("{{UserAddress}}", UserAddress);
                body = body.Replace("{{UserPhoneNumber}}", UserPhoneNumber);
                body = body.Replace("{{SubOrderTotal}}", SubOrderTotal);
                body = body.Replace("{{OrderTotal}}", OrderTotal);
                body = body.Replace("{{ProductOrder}}", ProductOrder);
                body = body.Replace("{{Payment}}", OrderPayment);
                body = body.Replace("{{Delivery}}", OrderDelivery);
                body = body.Replace("{{DeliveryId}}", DeliveryId);

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
        //Áp dụng code giảm giá sản phẩm
        public ActionResult UseDiscountCode(string code,double total_price)
        {
            var discount = db.Discounts.SingleOrDefault(d => d.discounts_code == code);
            if (discount != null)
            {
                //mã giảm giá phải nằm trong thời gian giảm giá và số lượng != 0
                if (discount.discount_start < DateTime.Now && discount.discount_end > DateTime.Now && discount.quantity !="0")
                {
                    if (discount.discounts_type == 3)
                    {
                        double discount_calc = ((total_price * discount.discount_price) / 100);
                        if (discount_calc >= discount.discount_max )
                        {
                            discount.discount_price = discount.discount_max;
                        }
                        else
                        {
                            discount.discount_price = (total_price * discount.discount_price) / 100;
                        }
                    }
                    else
                    {
                        discount.discount_price = discount.discount_price;
                    }
                    Session["Discount"] = discount.discount_price;
                    Session["Discountcode"] = discount.discounts_code.Trim();
                    return Json(new { success = true, discountPrice = discount.discount_price}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, discountPrice = 0 }, JsonRequestBehavior.AllowGet);
        }
        //Trường 
        private Tuple<List<Product>, List<int>> GetCart()
        {
            var cart = Request.Cookies.AllKeys.Where(c => c.IndexOf("product_") == 0);
            var productIds = new List<int>();
            var quantities = new List<int>();
            var errorProduct = new List<string>();
            var cValue = "";
            foreach (var item in cart)
            {
                var tempArr = item.Split('_');
                if (tempArr.Length != 2)
                {
                    errorProduct.Add(item);
                    continue;
                }
                cValue = Request.Cookies[item].Value;
                productIds.Add(Convert.ToInt32(tempArr[1]));
                quantities.Add(Convert.ToInt32(String.IsNullOrEmpty(cValue) ? "0" : cValue));
            }
            // Select sản phẩm để hiển thị
            var listProduct = new List<Product>();
            foreach (var id in productIds)
            {
                var product = db.Products.SingleOrDefault(p => p.status == "1" && p.product_id == id);
                if (product != null)
                {
                    listProduct.Add(product);
                }
                else
                {              
                    errorProduct.Add("product-" + id);
                    quantities.RemoveAt(productIds.IndexOf(id));
                }
            }
            
            foreach (var err in errorProduct)
            {
                Response.Cookies[err].Expires = DateTime.Now.AddDays(-1);
            }
            return new Tuple<List<Product>, List<int>>(listProduct, quantities);
        }
         
        public void BannerGlobal()
        {
            ViewBag.BannerTopHorizontal = db.Banners.OrderByDescending(m => Guid.NewGuid()).Where(m => m.banner_start < DateTime.Now && m.banner_end > DateTime.Now && m.status == "1" && m.banner_type == 3).Take(8).ToList();
        }
    }
}