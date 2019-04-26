using System;
using System.Collections.Generic;
using System.Linq;
using StockReplenishment.CommonService;
using StockReplenishment.SalesOrderService;
using StockReplenishment.SqlDAL;
using StockReplenishment.RopesDAL;
using StockReplenishment.Models.Models;
using StockReplenishment.Enumerations;

namespace StockReplenishment.ReplenishmentCalculatorService
{
    public class ReplenishmentCalculatorService : CommonService.CommonService {

        public ReplenishmentCalculatorService(StockReplenishmentEntities db, RopesDAL.RopesDAL ropesEntities = null) : base(db, ropesEntities) { }

        Store currentStore = null;

        ProductService.ProductService _productService = null;
        ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }

        public Error CreateOrders(int unixDate) {
            // On entry, this method requires that:
            //      Products have been updated from Erply
            //      Stock on hand has been updated from Erply
            //      Stock in transit been updated from Erply
            var error = new Error();
            int numSales = 0;

            try {
                LogService.WriteLog(TransType.Replenishment, null, null, "Creating Sales Orders for Ropes");

                ropesDb.Open();

                var sos = new SalesOrderService.SalesOrderService(db);

                List<SalesOrderItem> salesItems = new List<SalesOrderItem>();

                // Find all ProductsSold which need ordering, ordered by storeid.
                // This will be all items which have appeared since we last ran.
                foreach (var ps in db.FindProductsSold(unixDate)
                                     .ToList()) {
                    var productSold = db.RefreshEntity(ps);

                    if (currentStore == null || productSold.StoreId != currentStore.Id || salesItems.Count >= 50) {
                        // Found another store or the order has reached 50 items
                        if (currentStore != null) {
                            if (salesItems.Count() > 0) {
                                // Only create a sale if it has items
                                LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"Creating Sale for {salesItems.Count()} item(s) for Store: {currentStore.Name}");
                                numSales++;
                                error = sos.CreateSales(salesItems);
                                if (error.IsError) break;
                            }
                        }
                        salesItems = new List<SalesOrderItem>();
                    }
                    currentStore = productSold.Store;

                    if (currentStore.Range == null) {
                        LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"Warning: Store {currentStore.Name} has no Range defined - not replenishing product {productSold.ProductCode}");

                    } else {
                        // Search the store's range for the product.
                        // The presence of a record indicates that it is a member of the range and is to be replenished
                        var product = db.FindProduct(productSold.ProductCode);
                        if (product == null) {
                            LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"  Error: Product {productSold.ProductCode} could not be found in the Product table!");

                        } else {
                            if (currentStore.Range
                                            .ProductRanges
                                            .Where(pr => pr.ProductId == product.Id)
                                            .Count() > 0) {
                                // ProductRange record found, so product is in range and must be replenished

                                // Create an order item
                                var salesOrderItem = CreateOrderItem(productSold);

                                // Add the item to the order
                                if (salesOrderItem != null && salesOrderItem.Quantity > 0) {
                                    LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"  Adding Product {productSold.ProductCode} / {productSold.ProductName} to order");
                                    salesItems.Add(salesOrderItem);
                                } else {
                                    LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"  Product {productSold.ProductCode} / {productSold.ProductName} has sufficient stock - not replenishing");
                                }

                            } else {
                                LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"  Warning: Product {productSold.ProductCode} / {productSold.ProductName} not in range - not replenishing");
                            }
                        }
                    }
                }

                if (!error.IsError && salesItems.Count() > 0) {
                    // Only create a sale if it has items
                    LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"Creating Sale for {salesItems.Count()} item(s) for Store: {currentStore.Name}");
                    numSales++;
                    error = sos.CreateSales(salesItems);
                }
                if (!error.IsError) error = sos.SendQueuedFiles();

                if (currentStore != null) {
                    LogService.WriteLog(TransType.Replenishment, currentStore.Id, null, $"{numSales} Sales Order(s) created");
                } else {
                    LogService.WriteLog(TransType.Replenishment, null, null, $"{numSales} Sales Order(s) created");
                }

            } catch (Exception e) {
                error.SetError(e);
            }

            ropesDb.Close();

            return error;
        }

        public SalesOrderItem CreateOrderItem(ProductsSold productSold) {

            SalesOrderItem salesOrderItem = null;

            var product = db.FindProduct(productSold.ProductCode);
            if (product != null) {
                // Update the product pack size
                var rp = ropesDb.FindProduct(product.ProductCode);
                if (rp == null) {
                    LogService.WriteLog(TransType.Replenishment, productSold.StoreId, null, $"Error: Product {product.ProductCode} not found in ROPES! Using pack size of {product.PackSize}");

                } else {
                    product.PackSize = rp.MSQ;
                    db.InsertOrUpdateProduct(product, false);

                    LogService.WriteLog(TransType.Replenishment, productSold.StoreId, product.Id, $"Product {product.ProductCode} updated with pack size: {product.PackSize}");
                }

                salesOrderItem = new SalesOrderItem {
                    Brand = "",
                    SaleDate = DateTimeOffset.Now,
                    Number = "",
                    StoreId = productSold.StoreId,
                    ProductCode = productSold.ProductCode,
                    ProductName = productSold.ProductName,
                    Quantity = productSold.Qty,
                    CurrencyCode = "",
                    Price = 0,
                    Notes = "",
                    MSQ = (product.PackSize < 1 ? 1 : product.PackSize)
                };

                // We now apply the 'algorithm' to work out the quantity to be ordered:

                // The following method calculates: totalStock = stockOnHand - stockInTransit
                int stockOnHand = 0,
                    stockInTransit = 0;
                ProductService.GetTotalStock(currentStore, product, ref stockOnHand, ref stockInTransit);
                var totalStock = stockOnHand + stockInTransit;

                int sales = productSold.Qty;
                decimal forecastFactor = (currentStore.ForecastFactor == null ? 0 : currentStore.ForecastFactor.Value);

                // Formula take from spreadsheet provided by Emma Shaw
                // =IF(CEILING(F4-(C4+D4)+(A4*B4),E4)<0,0,CEILING(F4-(C4+D4)+(A4*B4),E4))
                // =IF(CEILING(product.Mpl-(totalStock)+(sales*forecastFactor),product.PackSize)<0,0,CEILING(product.Mpl-(totalStock)+(sales*forecastFactor),product.PackSize))
                //salesOrderItem.Quantity = Math.Max(0, (int)Ceiling(product.Mpl - (totalStock) + (sales * forecastFactor), product.PackSize));
                salesOrderItem.Quantity = CalculateOrderQty(product.Mpl, totalStock, sales, forecastFactor, product.PackSize);

                LogService.WriteLog(TransType.Replenishment, productSold.StoreId, product.Id, $"Product {productSold.ProductCode} / Sales:{sales} ForecastFactor:{forecastFactor} SOH:{stockOnHand} SIT:{stockInTransit} TotalStock:{totalStock} PackSize:{product.PackSize} MPL:{product.Mpl} OrderQty:{salesOrderItem.Quantity}");
            }

            return salesOrderItem;
        }

        public int CalculateOrderQty(int mpl, int totalStock, int sales, decimal forecastFactor, int packSize) {
            return (int)Ceiling(mpl - (totalStock) + (sales * forecastFactor), packSize);
        }

        public decimal Ceiling(decimal value, decimal significance) {
            if(value < 0) {
                return 0;
            } else if ((value % significance) != 0) {
                return ((int)(value / significance) * significance) + significance;
            } else {
                return value;
            }
        }
    }
}
