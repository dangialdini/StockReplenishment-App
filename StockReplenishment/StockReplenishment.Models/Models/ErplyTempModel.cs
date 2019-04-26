using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    public class ErplyTempModel {
        public int Id { get; set; } = 0;
        public int ErplyStoreId { get; set; } = 0;
        public string ErplyStoreName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Qty { get; set; } = 0;
        public DateTime SaleDate { get; set; }

    }
}
