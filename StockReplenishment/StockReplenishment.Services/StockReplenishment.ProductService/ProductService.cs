using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.ErplyService;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using StockReplenishment.Enumerations;
using Newtonsoft.Json.Linq;

namespace StockReplenishment.ProductService
{
    public partial class ProductService : CommonService.CommonService {
        public ProductService(StockReplenishment.SqlDAL.StockReplenishmentEntities dbEntities, RopesDAL.RopesDAL ropesEntities = null) : base(dbEntities, ropesEntities) { }

        private List<Range> AllRanges = null;

        #region Public methods

        public List<ProductModel> GetProducts() {
            List<ProductModel> model = new List<ProductModel>();
            AllRanges = db.FindRanges().ToList();
            var allItems = db.FindProducts();

            foreach(var item in allItems) {
                model.Add(mapToModel(item));
            }

            return model;
        }

        public Error UpdateProduct(ProductModel model) {
            Error error = new Error();
            
            foreach (var pr in model.ProductRanges) {
                ProductRange tempPr = null;
                if (pr.Id != 0) tempPr = db.FindProductRange(pr.Id); // Check to see if record is in db
                if (tempPr == null) { // NOT in db
                    if (pr.IsMember) {
                        tempPr = new ProductRange();
                        tempPr.ProductId = pr.ProductId;
                        tempPr.RangeId = pr.RangeId;

                        db.InsertOrUpdateProductRange(tempPr);
                    }
                } else { // IN db
                    if (!pr.IsMember) {
                        db.DeleteProductRange(tempPr.Id);
                    }
                }
                
            }

            Product temp = db.FindProduct(model.Id);
            temp.Mpl = model.Mpl;

            db.InsertOrUpdateProduct(temp, false);

            return error;
        }

        public Error UpdateProducts(EAPI api, int unixDate) {         // Changes since last run date
            var error = new Error();

            try {
                // Update the product list from Erply
                int     pageSize = 100,
                        pageNo = 1,
                        numProducts = 0;
                string  request = "getProducts";

                LogService.WriteLog(TransType.ProductUpdate, null, null, "Updating Products from Erply");

                var dict = createGetProductsParameters(pageNo, pageSize, unixDate);

                JObject json = api.sendRequest(request, dict);
                if (json != null) {
                    error = GetError(json["status"]["errorCode"].ToString(), json["status"]["responseStatus"].ToString());
                    int totalRecs = (int)json["status"]["recordsTotal"];
                    int numPages = totalRecs / pageSize;
                    if (numPages * pageSize < totalRecs) numPages++;

                    LogService.WriteLog(TransType.ProductUpdate, null, null, $"{totalRecs} Product(s) in {numPages} page(s) found");

                    while (!error.IsError && pageNo <= numPages) {
                        int numRecs = (int)json["status"]["recordsInResponse"];

                        for (int i = 0; i < numRecs; i++) {
                            int productId = Convert.ToInt32((int)json["records"][i]["productID"]);

                            bool bAdd = false;

                            var rp = ropesDb.FindProduct(json["records"][i]["code"].ToString());

                            var product = db.FindProduct(productId);
                            if (product == null) {
                                product = new Product {
                                    Id = productId,
                                    ProductCode = json["records"][i]["code"].ToString(),
                                    ProductName = json["records"][i]["name"].ToString(),
                                    PackSize = (rp == null ? 1 : rp.MSQ),
                                    Mpl = 0
                                };
                                bAdd = true;
                            } else {
                                product.ProductCode = json["records"][i]["code"].ToString();
                                product.ProductName = json["records"][i]["name"].ToString();
                                product.PackSize = (rp == null ? 1 : rp.MSQ);
                            }
                            db.InsertOrUpdateProduct(product, bAdd);
                            numProducts++;
                        }

                        pageNo++;
                        if (pageNo <= numPages) {
                            // Get the next page
                            dict = createGetProductsParameters(pageNo, pageSize, unixDate);

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
                    LogService.WriteLog(TransType.ProductUpdate, null, null, $"{numProducts} Product(s) retrieved");

                } else {
                    error.SetError($"Error: A NULL JSON object was returned by {request}!");
                }

            } catch(Exception e) {
                error.SetError(e);
            }
            return error;
        }

        public void GetTotalStock(Store store, Product product, ref int stockOnHand, ref int stockInTransit) {
            stockOnHand = stockInTransit = 0;

            if (store != null && product != null) {
                var storeStock = db.FindStoreStock(store.Id, product.Id);
                if (storeStock != null) {
                    stockOnHand = storeStock.StockOnHand;
                    stockInTransit = storeStock.StockinTransit;
                }
            }
        }

        #endregion

        #region Private methods

        private ProductModel mapToModel(Product entity) {
            var model = new ProductModel {
                Id = entity.Id,
                ProductCode = entity.ProductCode,
                ProductName = entity.ProductName,
                MSQ = entity.PackSize,
                Mpl = entity.Mpl,
                ProductRanges = new List<ProductRangeModel>()
            };
            foreach(var range in AllRanges) {
                var pr = entity.ProductRanges.Where(p => p.RangeId == range.Id).FirstOrDefault();
                if(pr != null) {
                    model.ProductRanges.Add(mapToModel(pr));
                } else {
                    model.ProductRanges.Add(new ProductRangeModel {
                        Id = 0,
                        ProductId = entity.Id,
                        RangeId = range.Id,
                        RangeName = range.Name,
                        IsMember = false
                    });
                }
            }

            return model;
        }

        private ProductRangeModel mapToModel(ProductRange entity) {
            var model = new ProductRangeModel {
                Id = entity.Id,
                ProductId = entity.ProductId,
                RangeId = entity.RangeId,
                RangeName = entity.Range.Name,
                IsMember = true
            };
            return model;
        }

        Dictionary<string, object> createGetProductsParameters(int pageNo, int pageSize, int unixDate) {
            var dict = new Dictionary<string, object>();
            dict.Add("pageNo", pageNo);
            dict.Add("recordsOnPage", pageSize);
            dict.Add("changedSince", unixDate);
            return dict;
        }
        #endregion
    }
}
