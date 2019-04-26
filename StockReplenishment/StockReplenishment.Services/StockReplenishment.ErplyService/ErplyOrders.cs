using Newtonsoft.Json.Linq;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.ErplyService {
    public partial class ErplyService {

        #region Public methods
        #endregion

        #region Private methods

        private JObject GetErplyOrders(EAPI api, int pageSize, int pageNo, int dateLastRun) {

            var dict = new Dictionary<string, object>();

            dict.Add("changedSince", dateLastRun);
            dict.Add("pageNo", pageNo);
            dict.Add("recordsOnPage", pageSize);
            dict.Add("getRowsForAllInvoices", 1);
            dict.Add("getAddedTimestamp", 1);

            return api.sendRequest("getSalesDocuments", dict);
        }

        #endregion
    }
}
