using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    public class ProductRangeModel {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int RangeId { get; set; }
        public string RangeName { get; set; }
        public bool IsMember { get; set; }
    }
}
