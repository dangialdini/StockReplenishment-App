using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockReplenishment.Enumerations;
using StockReplenishment.ErplyService;
using StockReplenishment.Models.Models;
using StockReplenishment.Models.ViewModels;
using StockReplenishment.SqlDAL;
using StockReplenishment.StoreService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockReplenishment.UI.Api
{
    public class StoresController : ApiController
    {

        #region Private methods

        protected StockReplenishmentEntities db = new StockReplenishmentEntities();

        private StoreService.StoreService _storeService;
        protected StoreService.StoreService StoreService {
            get {
                if (_storeService == null) _storeService = new StoreService.StoreService(db);
                return _storeService;
            }
        }

        private ProductService.ProductService _productService;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
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

        #endregion


        [Route("api/Stores/GetStores")]
        public IHttpActionResult GetStores() {
            var stores = StoreService.GetStores();

            foreach (var store in stores) {
                if (store.RangeId != null && store.RangeId != 0) store.Range = ProductService.GetRange(store.RangeId.Value);
            }

            return Ok(stores);
        }

        [Route("api/Stores/UpdateStores")]
        public IHttpActionResult UpdateStores(StoresViewModel stores) {
            StoreListModel model = new StoreListModel();
            model.Stores = JsonConvert.DeserializeObject<List<StoreModel>>(stores.Stores);

            foreach(var store in model.Stores) {
                StoreService.UpdateStores(store);
            }
            
            return Ok(stores);
        }

        [Route("api/Stores/GetProductRanges")]
        public IHttpActionResult GetProductRanges() {
            var ranges = ProductService.GetRanges();
            ranges.Insert(0, new RangeModel { Id = 0, Name = "" });
            return Ok(ranges);
        }

        [Route("api/Stores/RefreshStoresFromErply")]
        public IHttpActionResult RefreshStoresFromErply() {
            ErplyService.EAPI api = new ErplyService.EAPI();
            var error = ErplyService.GetStores(api);
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
