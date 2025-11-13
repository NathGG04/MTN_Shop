using System;
using System.Collections.Generic;
using NTM_SHOP.Models;
#nullable disable

namespace NTM_SHOP.Models
{
    public partial class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? OrderNumber { get; set; }
        public int? Quantity { get; set; }
        public int? Discount { get; set; }
        public int? TotalMoney { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Price { get; set; }

        public virtual Order Order { get; set; }
        public Product product { get; set; }


    }
}
