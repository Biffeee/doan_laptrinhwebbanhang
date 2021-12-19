/*
 * Format giỏ hàng: product_{id}={quantity}
 */
// Khởi tạo giỏ hàng khi vào trang giỏ hàng
$(window).ready(function (ev) {
	$("#cartCount").text(Cookie.countWithPrefix("product"));
});
// Button xóa sản phẩm khỏi giỏ hàng
$(".js-delete").click(function (ev) {
	bootbox.confirm({
		message: "Xoá sản phẩm?",
		buttons: {
			confirm: {
				label: 'Xoá',
				className: 'btn-danger'
			},
			cancel: {
				label: 'Quay lại',
				className: 'btn-secondary'
			}
		},
		callback: function (result) {
			if (result) {
				var id = $(ev.currentTarget).data("id");
				var item = $(ev.currentTarget).closest(".item");
				item.remove();
				Cookie.remove("product_" + id);
				var productCount = Cookie.countWithPrefix("product")
				$("#cartCount").text(productCount);
				$("#lblCartCount").text(productCount == 0 ? "0" : productCount);
				updateOrderPrice();
			}
		}
	})
});
//
//cập nhật giỏ hảng
function updateOrderPrice() {
	var quanInputs = $("input.qty-input");
	var newTotal = 0;
	var totalWithFee;
	quanInputs.each(function (i, e) {
		var price = Number($(e).data('price'));
		var quan = Number($(e).val());
		newTotal += price * quan;
	});
	var eleDiscount = $("#discount");
	var discount = 0;
	if (eleDiscount.attr("data-price") == null) {
		totalWithFee = newTotal + 30000;
	}
	else {
		if (eleDiscount.attr("data-price") < (newTotal)) {
			discount = Number(eleDiscount.attr("data-price"));
			totalWithFee = newTotal + 30000 - discount;
		}
		else {
			discount = Number(eleDiscount.attr("data-price"));
			totalWithFee = 30000;
		}
	}
	totalWithFee += "";
	newTotal += "";
	discount += "";
	var regex = /(\d)(?=(\d{3})+(?!\d))/g;
	$("#totalPrice").text(newTotal.replace(regex, "$1.") + "₫");
	$("#total_price").val(newTotal);
	$("#totalPriceWithFee").text(totalWithFee.replace(regex, "$1.") + "₫");
	$("#discount").text(discount.replace(regex, "$1.") + "₫");
};
//được sử dụng khi bấm nút thanh toán
$(".js-checkout").click(function (ev) {
	ev.preventDefault();
	var self = $(this);
	console.log(self.data('title'));
	$.get("/account/userlogged", {},
		function (isLogged, textStatus, jqXHR) {
			if (!isLogged) {
				//gọi action đăng nhập khi người dùng bấm thanh toán mà chưa đăng nhập hệ thống
				bootbox.confirm({
					message: "!Vui lòng đăng nhập để thực hiện chức năng thanh toán",
					buttons: {
						confirm: {
							label: 'Đăng nhập',
							className: 'btn-info'
						},
						cancel: {
							label: 'Quay lại',
							className: 'btn-secondary'
						}
					},
					callback: function (result) {
						if (result) {
							window.location = "/Account/SignIn";
						}
					}
				});
			}
			else {
				location.href = ev.currentTarget.href;
			}
		},
		"json"
	);
});
//Sử dụng code giảm giá
$(".btn-submitcoupon").click(function (ev) {
	var input = $(ev.currentTarget).prev();
	var _code = input.val().trim();
	var _totalprice = $("#total_price").val().trim();
	var newTotal = 0;
	var quanInputs = $("input.qty-input");
	quanInputs.each(function (i, e) {
		var price = Number($(e).data('price'));
		var quan = Number($(e).val());
		newTotal += price * quan;
	});
	
	if (_code == "") {
		const Toast = Swal.mixin({
			toast: true,
			position: 'top',
			showConfirmButton: false,
			timer: 2000,
			didOpen: (toast) => {
				toast.addEventListener('mouseenter', Swal.stopTimer)
				toast.addEventListener('mouseleave', Swal.resumeTimer)
			}
		})
		Toast.fire({
			icon: 'warning',
			title: 'Vui lòng nhập mã giảm giá'
		})
    }
	if (_code.length) {
		$.getJSON("/cart/UseDiscountCode", { code: _code, total_price: _totalprice},
			function (data, textStatus, jqXHR) {
				if (data.success) {
					$("#discount").attr("data-price", data.discountPrice);
                    updateOrderPrice();
                    const Toast = Swal.mixin({
                        toast: true,
                        position: 'top',
                        showConfirmButton: false,
                        timer: 1500,
                        didOpen: (toast) => {
                            toast.addEventListener('mouseenter', Swal.stopTimer)
                            toast.addEventListener('mouseleave', Swal.resumeTimer)
                        }
                    })
                    Toast.fire({
                        icon: 'success',
                        title: 'Đã áp dụng mã giảm giá'
					})
                    // Hiển thị thông báo
                } else {
                    const Toast = Swal.mixin({
                        toast: true,
                        position: 'top',
                        showConfirmButton: false,
                        timer: 1500,
                        didOpen: (toast) => {
                            toast.addEventListener('mouseenter', Swal.stopTimer)
                            toast.addEventListener('mouseleave', Swal.resumeTimer)
                        }
                    })
                    Toast.fire({
                        icon: 'error',
                        title: 'Đã có lỗi, không thể sử dụng mã giảm giá'
                    })
                }
            }
		);
	}
})
// Button giảm số lượng
$(".btn-dec").click(function (ev) {
	$(".btn-inc").removeClass('no-pointer-events');
	var quanInput = $(ev.currentTarget).next();
	var id = quanInput.data("id");
	var quan = Number(quanInput.val());
	if (quan > 1) {
		quan = quan - 1;
		Cookie.set("product_" + id, quan, 2);
		quanInput.val(quan);
		updateOrderPrice();
	}
});
//Nhập số lượng và thay đổi
function Update_quan_mouse_ev() {
	var id = $('.qty-input').data("id");
	$(".btn-inc").removeClass('no-pointer-events');
	var quan = $('.qty-input').val();
	if (quan != "") {
		Cookie.set("product_" + id, quan, 2);
		updateOrderPrice();
	}
}
$(".qty-input").mouseleave(function () {
	Update_quan_mouse_ev()
});
$(".qty-input").mouseover(function () {
	Update_quan_mouse_ev()
});
$(".qty-input").change(function (ev) {
	Update_quan_mouse_ev()
})
$(".qty-input").mouseout(function (ev) {
	Update_quan_mouse_ev()
})
// Button tăng số lượng
$(".btn-inc").click(function (ev) {
	var maxquan = $('.qty-input').attr('max');
	var quanInput = $(ev.currentTarget).prev();
	var id = quanInput.data("id");
	var quan = Number(quanInput.val());
	if (quan < 1) {
		quan = 1;
		Cookie.set("product_" + id, quan, 2);
		quanInput.val(quan);
		updateOrderPrice();
	}
	else if (quan >= maxquan) {		
		$(".btn-inc").addClass('no-pointer-events');
		const Toast = Swal.mixin({
			toast: true,
			position: 'top',
			showConfirmButton: false,
			timer: 2000,
			didOpen: (toast) => {
				toast.addEventListener('mouseenter', Swal.stopTimer)
				toast.addEventListener('mouseleave', Swal.resumeTimer)
			}
		})
		Toast.fire({
			icon: 'error',
			title: 'Số lượng đã đạt giới hạn'
		})
    }
	else {
		quan = quan + 1;
		Cookie.set("product_" + id, quan, 2);
		quanInput.val(quan);
		updateOrderPrice();
    }
});