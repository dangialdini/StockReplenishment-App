using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    public class StoreModel {
        public int Id { get; set; } = 0;
        public int? RangeId { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
        public string Suburb { get; set; } = "";
        public string State { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string Country { get; set; } = "";
        public decimal ForecastFactor { get; set; } = 0;
        public int? Priority { get; set; } = null;
        public bool IsActive { get; set; } = false;
        public RangeModel Range { get; set; } = new RangeModel();
    }

    public class StoreListModel {
        public List<StoreModel> Stores { get; set; } = new List<StoreModel>();
    }
}
