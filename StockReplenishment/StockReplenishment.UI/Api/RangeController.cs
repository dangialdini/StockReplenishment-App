using Newtonsoft.Json;
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
    public class RangeController : ApiController
    {
        private ProductService.ProductService _productService;
        protected StockReplenishmentEntities db = new StockReplenishmentEntities();
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }



        [Route("api/Range/GetRanges")]
        public IHttpActionResult GetRanges() {
            var pr = ProductService.GetRanges();

            return Ok(pr);
        }

        [Route("api/Range/CreateRange")]
        public IHttpActionResult CreateRange(RangeViewModel range) {
            RangeListModel model = new RangeListModel();
            model.Ranges = JsonConvert.DeserializeObject<List<RangeModel>>(range.Range);

            foreach(var pr in model.Ranges) {
                ProductService.InsertOrUpdateRange(pr);
            }

            return Ok(range);
        }

        [Route("api/Range/UpdateRange")]
        public IHttpActionResult UpdateRange(RangeViewModel range) {
            RangeListModel model = new RangeListModel();
            model.Ranges = JsonConvert.DeserializeObject<List<RangeModel>>(range.Range);

            foreach (var pr in model.Ranges) {
                ProductService.InsertOrUpdateRange(pr);
            }

            return Ok(range);
        }

        [Route("api/Range/DeleteRange")]
        public IHttpActionResult DeleteRange(RangeViewModel range) {
            RangeListModel model = new RangeListModel();
            model.Ranges = JsonConvert.DeserializeObject<List<RangeModel>>(range.Range);

            ProductService.DeleteRange(model.Ranges[0]);

            return Ok();
        }
    }
}
