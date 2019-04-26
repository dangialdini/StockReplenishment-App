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

        public Error GetStores(EAPI api) {
            var error = new Error();

            try {
                int pageSize = 100,
                        pageNo = 1;
                string request = "getWarehouses";

                LogService.WriteLog(TransType.StoreUpdate, null, null, "Updating Store list");

                var dict = createGetWarehousesParameters(pageNo, pageSize);

                JObject json = api.sendRequest(request, dict);
                if (json != null) {
                    error = GetError(json["status"]["errorCode"].ToString(), json["status"]["responseStatus"].ToString());

                    int totalRecs = (int)json["status"]["recordsTotal"];
                    int numPages = totalRecs / pageSize;
                    if (numPages * pageSize < totalRecs) numPages++;

                    while (!error.IsError && pageNo <= numPages) {
                        int numRecs = (int)json["status"]["recordsInResponse"];

                        for (int i = 0; i < numRecs; i++) {
                            int storeId = Convert.ToInt32((int)json["records"][i]["warehouseID"]);
                            var store = db.FindStore(storeId);

                            if (store == null) store = new Store { Id = storeId, ForecastFactor = 1, Priority = 999 };
                            store.Name = json["records"][i]["name"].ToString();
                            store.Address1 = json["records"][i]["street"].ToString();
                            store.Address2 = json["records"][i]["address2"].ToString();
                            store.Suburb = json["records"][i]["city"].ToString();
                            store.State = json["records"][i]["state"].ToString();
                            store.Postcode = json["records"][i]["ZIPcode"].ToString();
                            store.Country = json["records"][i]["country"].ToString();

                            if (json["records"][i]["storeGroups"].ToString().ToLower().IndexOf("closed down") != -1 ||
                               store.Name.ToLower().IndexOf("closed") == 0) {
                                store.IsActive = false;
                            } else {
                                store.IsActive = true;
                            }
                            db.InsertOrUpdateStore(store);

                            LogService.WriteLog(TransType.StoreUpdate, store.Id, null, $"Updated store: #{store.Id} Name:{store.Name} Addrs1:{store.Address1} Addrs2:{store.Address2} Suburb:{store.Suburb} State:{store.State} Postcode:{store.Postcode} Country:{store.Country}");
                        }

                        pageNo++;
                        if (pageNo <= numPages) {
                            // Get the next page
                            dict = createGetWarehousesParameters(pageNo, pageSize);

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

                } else {
                    error.SetError($"Error: A NULL JSON object was returned by {request}!");
                }

            } catch (Exception e) {
                error.SetError(e);
            }
            return error;
        }

        Dictionary<string, object> createGetWarehousesParameters(int pageNo, int pageSize) {
            var dict = new Dictionary<string, object>();
            dict.Add("pageNo", pageNo);
            dict.Add("recordsOnPage", pageSize);
            return dict;
        }
    }
}
