using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    // A list of this class is returned by the Algorithm process
    // and is fed as input into the order creation process.
    public class SalesOrderItem {
        public string Brand { set; get; }
        public DateTimeOffset SaleDate { set; get; }
        public string Number { set; get; }          // Order number
        public int StoreId { set; get; }
        public string ProductCode { set; get; }
        public string ProductName { set; get; }
        public int Quantity { set; get; }
        public string CurrencyCode { set; get; }
        public decimal Price { set; get; }
        public string Notes { set; get; } = "";
        public int MSQ { set; get; } = 1;
    }
}
