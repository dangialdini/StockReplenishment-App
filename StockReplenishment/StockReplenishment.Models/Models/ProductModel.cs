using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    public class ProductModel {
        public int Id { set; get; }
        public string ProductCode { set; get; }
        public string ProductName { set; get; }
        public int MSQ { set; get; }
        public int Mpl { get; set; }

        public List<ProductRangeModel> ProductRanges { get; set; }

    }

    public class ProductListModel {
        public List<ProductModel> Products { get; set; } = new List<ProductModel>();
    }
}
