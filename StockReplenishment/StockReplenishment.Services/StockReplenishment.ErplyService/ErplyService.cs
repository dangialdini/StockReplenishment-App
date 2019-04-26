using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StockReplenishment.CommonService;
using StockReplenishment.LogService;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using StockReplenishment.Enumerations;

namespace StockReplenishment.ErplyService
{
    public partial class ErplyService : CommonService.CommonService {

        #region Contruction

        public ErplyService(StockReplenishmentEntities dbEntities, RopesDAL.RopesDAL ropesEntities = null) : base(dbEntities, ropesEntities) {
        }

        #endregion

        #region Public methods

        public int ProcessErply(EAPI api) {
            // Get Stores data from Erply
            var error = GetStores(api);
            if (error.IsError) LogService.WriteLog(TransType.Erply, null, null, error.Message);

            // Get SalesOrders from Erply
            int pageSize = 100;
            int pageNo = 1;

            var dateLastRun = db.GetLastOrderExtractTime();
            LogService.WriteLog(TransType.Erply, null, null, "Date Last Run: " + dateLastRun.ToString());

            JObject erplyOrders = null;
            erplyOrders = GetErplyOrders(api, pageSize, pageNo, dateLastRun);
            var dateThisRun = SaveOrdersToTempTables(erplyOrders);

            // Check if number of orders exceeds the pageSize of the ErplyAPI
            // If so, loop through and process the remaining orders
            int numberOfErplyOrders = erplyOrders["records"].Count();
            int totalRecs = (int)erplyOrders["status"]["recordsTotal"];

            if (numberOfErplyOrders > 0) {
                do {
                    pageNo++;
                    erplyOrders = GetErplyOrders(api, pageSize, pageNo, dateLastRun);
                    SaveOrdersToTempTables(erplyOrders);

                    numberOfErplyOrders = erplyOrders["records"].Count();
                } while (numberOfErplyOrders > 0);
            }

            db.UpdateLastOrderExtractTime(dateThisRun);

            LogService.WriteLog(TransType.Erply, null, null, "Received " + totalRecs + " Erply orders");

            return dateThisRun;
        }

        public void SaveTempToProductsSoldTable(int unixRunTimeStamp) {
            List<ErplyTempModel> products = null;
            products = GetErplyTemps();
            if (products.Count() > 0 || products != null) {
                foreach (var product in products) {
                    SaveTempToProductsSoldTable(product, unixRunTimeStamp);
                }
            }
            LogService.WriteLog(TransType.Erply, null, null, "Created " + products.Count() + " orders");
        }

        #endregion
    }
}
