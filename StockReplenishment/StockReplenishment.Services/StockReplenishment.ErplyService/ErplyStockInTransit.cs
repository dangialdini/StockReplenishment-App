using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using StockReplenishment.Enumerations;
using Newtonsoft.Json.Linq;

namespace StockReplenishment.ErplyService {
    public partial class ErplyService {

        public Error GetStockInTransit(EAPI api, int unixDate, int warehouseId) {         // Changes since last run date
            var error = new Error();

            try {
                int     pageSize = 100,
                        pageNo = 1,
                        numStockInTransit = 0;
                string  request = "getPurchaseDocuments";

                LogService.WriteLog(TransType.StockInTransit, null, null, "Updating Stock in Transit from Erply");

                var dict = createGetStockInTransitParameters(pageNo, pageSize, unixDate, warehouseId);

                JObject json = api.sendRequest(request, dict);
                if (json != null) {
                    error = GetError(json["status"]["errorCode"].ToString(), json["status"]["responseStatus"].ToString());

                    int totalRecs = (int)json["status"]["recordsTotal"];
                    int numPages = totalRecs / pageSize;
                    if (numPages * pageSize < totalRecs) numPages++;

                    LogService.WriteLog(TransType.StockInTransit, null, null, $"{totalRecs} records(s) in {numPages} page(s) found");

                    while (!error.IsError && pageNo <= numPages) {
                        int numRecs = (int)json["status"]["recordsInResponse"];

                        for (int i = 0; i < numRecs; i++) {
                            int row = 0;
                            int productId = 1;
                            while(productId > 0) {
                                try {
                                    productId = Convert.ToInt32((int)json["records"][i]["rows"][row]["productID"]);
                                } catch {
                                    productId = -1;
                                }
                                if(productId > 0) {
                                    int storeId = Convert.ToInt32((int)json["records"][i]["warehouseID"]);
                                    var store = db.FindStore(storeId);

                                    var product = db.FindProduct(productId);
                                    if (product == null) {
                                        product = new Product {
                                            Id = productId,
                                            ProductCode = json["records"][i]["rows"][row]["code"].ToString(),
                                            ProductName = json["records"][i]["rows"][row]["itemName"].ToString(),
                                            PackSize = 1,
                                            Mpl = 0
                                        };

                                        var oopsProduct = ropesDb.FindProduct(product.ProductCode);
                                        if (oopsProduct != null) product.PackSize = oopsProduct.MSQ;

                                        db.InsertOrUpdateProduct(product, true);
                                    }

                                    var storeStock = db.FindStoreStock(storeId, productId);
                                    if (storeStock == null) storeStock = new StoreStock { StoreId = storeId, ProductId = productId, StockOnHand = 0 };

                                    var stockInTransit = Convert.ToInt32(json["records"][i]["rows"][row]["amount"]);
                                    if (stockInTransit < 0) stockInTransit = 0;
                                    storeStock.StockinTransit = stockInTransit;
                                    db.InsertOrUpdateStoreStock(storeStock);
                                    numStockInTransit++;

                                    LogService.WriteLog(TransType.StockInTransit, storeId, productId, $"  {(store != null ? store.Name : "[Unknown Store]")} / {product.ProductCode} {product.ProductName}: {storeStock.StockinTransit}");
                                }
                                row++;
                            }                            
                        }

                        pageNo++;
                        if (pageNo <= numPages) {
                            // Get the next page
                            dict = createGetStockInTransitParameters(pageNo, pageSize, unixDate, warehouseId);

                            json = api.sendRequest(request, dict);
                            if (json != null) {
                                error = GetError(json["status"]["errorCode"].ToString(), json["status"]["responseStatus"].ToString());
                                totalRecs = (int)json["status"]["recordsTotal"];
                                numPages = totalRecs / pageSize;
                                if (numPages * pageSize < totalRecs) numPages++;
                            } else {
                                error.SetError($"Error: A NULL JSON object was returned by {request}!");
                            }
                        }
                    }
                    LogService.WriteLog(TransType.StockInTransit, null, null, $"{numStockInTransit} Stock in Transit record(s) retrieved");

                } else {
                    error.SetError($"Error: A NULL JSON object was returned by {request}!");
                }

            } catch(Exception e) {
                error.SetError(e);
            }
            return error;
        }

        Dictionary<string, object> createGetStockInTransitParameters(int pageNo, int pageSize, int unixDate, int warehouseId) {
            var dict = new Dictionary<string, object>();
            dict.Add("pageNo", pageNo);
            dict.Add("recordsOnPage", pageSize);
            dict.Add("changedSince", unixDate);
            dict.Add("getRowsForAllInvoices", "1");
            dict.Add("status", "PENDING");
            dict.Add("getCost", 1);
            if(warehouseId > 0) dict.Add("warehouseID", warehouseId);
            return dict;
        }
    }
}
