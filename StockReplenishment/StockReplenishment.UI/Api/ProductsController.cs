using Newtonsoft.Json;
using StockReplenishment.Enumerations;
using StockReplenishment.Models.Models;
using StockReplenishment.Models.ViewModels;
using StockReplenishment.SqlDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockReplenishment.UI.Api
{
    public class ProductsController : ApiController
    {
        protected StockReplenishmentEntities db = new StockReplenishmentEntities();
        protected RopesDAL.RopesDAL Ropesdb = new RopesDAL.RopesDAL();

        private ProductService.ProductService _productService;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db, Ropesdb);
                return _productService;
            }
        }

        private ErplyService.ErplyService _erplyService;
        protected ErplyService.ErplyService ErplyService {
            get {
                if (_erplyService == null) _erplyService = new ErplyService.ErplyService(db);
                return _erplyService;
            }
        }


        [Route("api/Products/GetProducts")]
        public IHttpActionResult GetProducts() {
            var products = ProductService.GetProducts();

            return Ok(products);
        }


        [Route("api/Products/UpdateProduct")]
        public IHttpActionResult UpdateProduct(ProductViewModel products) {
            ProductListModel model = new ProductListModel();
            model.Products = JsonConvert.DeserializeObject<List<ProductModel>>(products.Products);

            foreach(var product in model.Products) {
                ProductService.UpdateProduct(product);
            }

            return Ok(products);
        }

        [Route("api/Products/RefreshProductsFromErply")]
        public IHttpActionResult RefreshProductsFromErply() {
            ErplyService.EAPI api = new ErplyService.EAPI();
            DateTime unixYear0 = new DateTime(1970, 1, 1);
            long unixTimestamp = DateTime.Today
                                         .Subtract(unixYear0)
                                         .Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            var error = ProductService.UpdateProducts(api, (int)unixTimestamp);
            if (error.IsError) {
                error.Icon = ErrorIcon.None;
                var response = new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent(error.Message),
                    ReasonPhrase = error.Message
                };
                throw new HttpResponseException(response);
            } else {
                return Ok();
            }
        }
    }
}
