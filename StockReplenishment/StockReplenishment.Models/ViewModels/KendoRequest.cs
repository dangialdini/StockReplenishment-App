using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Models.ViewModels {
    public class KendoRequest {
        public int take { get; set; }
        public int skip { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<Sort> sort { get; set; }
        public Filter filter { get; set; }
    }

    public class Sort {
        public string field { get; set; }
        public string dir { get; set; }
    }

    public class Filter {
        public string logic { get; set; }
        public List<Filters> filters  { get; set; }
    }

    public class Filters {

        public string field { get; set; }
    }
}
