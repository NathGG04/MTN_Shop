using System;
using System.Collections.Generic;

namespace NTM_SHOP.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }

        // THÊM DẤU '?' VÌ CÁC CỘT NÀY CÓ THỂ LÀ NULL TRONG DB
        public string? Phone { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        // CỘT SALT CÓ GIÁ TRỊ NULL TRONG DB CỦA BẠN
        public string? Salt { get; set; }

        public bool Active { get; set; }

        // THÊM DẤU '?' CHO FULLNAME NẾU NÓ KHÔNG BẮT BUỘC TRONG DB
        public string? FullName { get; set; }

        public int? RoleId { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreateDate { get; set; }

        public virtual Role Role { get; set; }
    }
}