using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn_LapTrinhWeb.DTOs
{
    public class ChekcoutOrderDTO
    {
        public int payment_id { get; set; }
        
        public string payment_transaction { get; set; }
        [Required]
        [StringLength(20)]
        [MinLength(1, ErrorMessage = "Nhập Họ tên")]
        [MaxLength(20, ErrorMessage = "Họ tên không quá 20 ký tự")]
        public string checkout_username { get; set; }
        [StringLength(150)]
        [MinLength(1, ErrorMessage = "Nhập địa chỉ")]
        [MaxLength(150, ErrorMessage = "Địa chỉ không quá 150 ký tự")]
        public string checkout_address { get; set; }
        [StringLength(10)]
        [MinLength(10, ErrorMessage = "Số điện thoại phải đủ 10 chữ số")]
        [MaxLength(10, ErrorMessage = "Số điện thoại không quá 10 chữ số")]
        public string checkout_phone_number { get; set; }
        [StringLength(100)]
        [MinLength(1, ErrorMessage = "Nhập Email")]
        [MaxLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string checkout_email { get; set; }
        public int account_address_id { get; set; }
        public double total_price { get; set; }
    }
}