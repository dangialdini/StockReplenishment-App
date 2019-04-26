using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Enumerations {
    public enum TransType {
        ProductUpdate = 1,
        StockOnHand = 2,
        StockInTransit = 3,
        Replenishment = 4,
        Erply = 5,
        Application = 6,
        StoreUpdate = 7
    }
}
