using NTM_SHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.ModelViews
{
    public class CartItem
    {
        public Product product { get; set; }
        public int quantity { get; set; }
        public double TotalMoney => quantity * product.Price.Value;
    }
}
