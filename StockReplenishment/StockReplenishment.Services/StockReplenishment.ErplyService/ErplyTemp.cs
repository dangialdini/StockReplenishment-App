using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using StockReplenishment.Enumerations;
using Newtonsoft.Json.Linq;

namespace StockReplenishment.ErplyService {
    public partial class ErplyService {

        #region Public methods

        public List<ErplyTempModel> GetErplyTemps() {
            List<ErplyTempModel> temps = new List<ErplyTempModel>();

            var productsToOrder = db.ErplyTemps
                                    .GroupBy(et => new { et.ProductCode, et.ProductName, et.ErplyStoreId, et.ErplyStoreName })
                                    .Select(s => new ErplyTempModel {
                                        ProductCode = s.Key.ProductCode,
                                        ProductName = s.Key.ProductName,
                                        ErplyStoreId = s.Key.ErplyStoreId.Value,
                                        ErplyStoreName = s.Key.ErplyStoreName,
                                        Qty = s.Sum(sum => sum.Qty).Value
                                    })
                                    .ToList();

            foreach (var product in productsToOrder) {
                temps.Add(product);
            }

            return temps;
        }

        public void SaveTempToProductsSoldTable(ErplyTempModel temp, int unixTimeStamp) {
            ProductsSold entity = new ProductsSold {
                StoreId = temp.ErplyStoreId,
                UnixDateStamp = unixTimeStamp,
                ProductCode = temp.ProductCode,
                ProductName = temp.ProductName,
                //Minimum = ??
                //DFO = ??
                Qty = temp.Qty
            };

            db.InsertOrUpdateProductSold(entity);
        }

        public void CleanTempTable() {
            db.CleanErplyTempTable();
        }

        #endregion

        #region Private methods

        private int SaveOrdersToTempTables(JObject erplyOrders) {
            int dateThisRun = 0;

            db.BeginTransaction();
            try {
                dateThisRun = (int)erplyOrders["status"]["requestUnixTime"];

                foreach (var record in erplyOrders["records"]) {
                    foreach (var row in record["rows"]) {
                        ErplyTemp entity = new ErplyTemp {
                            ProductCode = row["code"].ToString(),
                            ProductName = row["itemName"].ToString(),
                            ErplyStoreId = (int)record["warehouseID"],
                            ErplyStoreName = record["warehouseName"].ToString(),
                            Qty = (int)row["amount"],
                            SaleDate = DateTime.ParseExact(record["date"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        };

                        db.InsertOrUpdateErplyTemp(entity);

                        var product = db.FindProduct(entity.ProductCode);
                        if (product == null) {
                            LogService.WriteLog(TransType.Erply, entity.ErplyStoreId, null, $"Received Sale for {entity.ProductCode} / {entity.ProductName}: Qty: {entity.Qty}");
                        } else {
                            LogService.WriteLog(TransType.Erply, entity.ErplyStoreId, product.Id, $"Received Sale for {entity.ProductCode} / {entity.ProductName}: Qty: {entity.Qty}");
                        }
                    }
                }

                db.CommitTransaction();
            } catch (Exception) {
                db.RollbackTransaction();
            }

            return dateThisRun;
        }

        #endregion
    }
}
