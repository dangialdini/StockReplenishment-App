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

        public Error GetStockOnHandForAllStores(EAPI api, int unixDate, int storeId) {
            // We have to make stock on hand requests for each store from Erply because
            // although its API does enable a blank warehouseID (meaning 'all warehouses') to be
            // supplied, the returned data contains no warehouseId so we can't work out which
            // warehouse each record belongs to.
            var error = new Error();

            // We only process stores with a range configured as we can't
            // order anything for them without the range being set
            foreach(var store in db.FindStores(true)
                                   .Where(s => storeId < 1 || s.Id == storeId)
                                   .ToList()) {
                error = GetStockOnHand(api, store, unixDate);
                if (error.IsError) break;
            }
            return error;
        }

        public Error GetStockOnHand(EAPI api, 
                                    Store store, 
                                    int unixDate) {         // Changes since last run date
            var     error = new Error();

            try {
                int     pageSize = 100,
                        pageNo = 1,
                        numStockOnHand = 0;
                string  request = "getProductStock";

                if (store.RangeId == null) {
                    LogService.WriteLog(TransType.StockOnHand, store.Id, null, "Warning: Not updating Stock on Hand for Erply store: " + store.Name + " because it has no Range set");

                } else {
                    LogService.WriteLog(TransType.StockOnHand, store.Id, null, "Updating Stock on Hand for Erply store: " + store.Name);

                    var dict = createGetStockOnHandParameters(store, pageNo, pageSize, unixDate);

                    JObject json = api.sendRequest(request, dict);
                    if (json != null) {
                        error = GetError(json["status"]["errorCode"].ToString(), json["status"]["responseStatus"].ToString());

                        int totalRecs = (int)json["status"]["recordsTotal"];
                        int numPages = totalRecs / pageSize;
                        if (numPages * pageSize < totalRecs) numPages++;

                        LogService.WriteLog(TransType.StockOnHand, store.Id, null, $"{totalRecs} records(s) in {numPages} page(s) found");

                        while (!error.IsError && pageNo <= numPages) {
                            int numRecs = (int)json["status"]["recordsInResponse"];

                            for (int i = 0; i < numRecs; i++) {
                                int productId = Convert.ToInt32((int)json["records"][i]["productID"]);
                                var product = db.FindProduct(productId);
                                if (product == null) {
                                    LogService.WriteLog(TransType.StockOnHand, store.Id, productId, $"Product Id #{productId} does not exist - creating it");

                                    product = new Product {
                                        Id = productId,
                                        ProductCode = productId.ToString(),
                                        ProductName = "Erply Product Id:" + productId.ToString(),
                                        PackSize = 1,
                                        Mpl = 0
                                    };

                                    var oopsProduct = ropesDb.FindProduct(product.ProductCode);
                                    if (oopsProduct != null) product.PackSize = oopsProduct.MSQ;

                                    db.InsertOrUpdateProduct(product, true);
                                }

                                var storeStock = db.FindStoreStock(store.Id, productId);
                                if (storeStock == null) storeStock = new StoreStock { StoreId = store.Id, ProductId = productId, StockinTransit = 0 };

                                var stockOnHand = (int)Convert.ToDecimal(json["records"][i]["amountInStock"]);
                                //if (stockOnHand < 0) stockOnHand = 0;   // Erply can return negative values
                                storeStock.StockOnHand = stockOnHand;
                                db.InsertOrUpdateStoreStock(storeStock);
                                numStockOnHand++;

                                LogService.WriteLog(TransType.StockOnHand, store.Id, productId, $"  {store.Name} / {product.ProductCode} {product.ProductName}: {storeStock.StockOnHand}");
                            }

                            pageNo++;
                            if (pageNo <= numPages) {
                                // Get the next page
                                dict = createGetStockOnHandParameters(store, pageNo, pageSize, unixDate);

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
                        LogService.WriteLog(TransType.StockOnHand, store.Id, null, $"{numStockOnHand} Stock on Hand record(s) retrieved");

                    } else {
                        error.SetError($"Error: A NULL JSON object was returned by {request}!");
                    }
                }

            } catch(Exception e) {
                error.SetError(e);
            }
            return error;
        }

        Dictionary<string, object> createGetStockOnHandParameters(Store store, int pageNo, int pageSize, int unixDate) {
            var dict = new Dictionary<string, object>();
            dict.Add("warehouseID", store.Id.ToString());
            dict.Add("pageNo", pageNo);
            dict.Add("recordsOnPage", pageSize);
            dict.Add("changedSince", unixDate);
            return dict;
        }
    }
}
