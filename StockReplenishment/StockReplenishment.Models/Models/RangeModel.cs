using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.Models {
    public class RangeModel {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class RangeListModel {
        public List<RangeModel> Ranges { get; set; } = new List<RangeModel>();
    }
}
