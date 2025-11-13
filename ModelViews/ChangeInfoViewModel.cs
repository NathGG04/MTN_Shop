using System.ComponentModel.DataAnnotations;

namespace NTM_SHOP.ModelViews
{
    public class ChangeInfoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ nhận hàng")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public int TinhThanh { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        public int QuanHuyen { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã")]
        public int PhuongXa { get; set; }
    }
}
