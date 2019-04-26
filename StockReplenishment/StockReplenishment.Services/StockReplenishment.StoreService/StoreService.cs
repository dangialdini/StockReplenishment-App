using StockReplenishment.CommonService;
using StockReplenishment.Models.Models;
using StockReplenishment.ProductService;
using StockReplenishment.SqlDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.StoreService
{
    public partial class StoreService : CommonService.CommonService {
        public StoreService(StockReplenishment.SqlDAL.StockReplenishmentEntities dbEntities) : base(dbEntities) {}

        #region Public methods

        public List<StoreModel> GetStores(bool activeOnly = false) {
            List<StoreModel> model = new List<StoreModel>();

            var allItems = db.FindStores(activeOnly);
            
            foreach(var item in allItems) {
                model.Add(mapToModel(item));
            }

            return model;
        }

        public Error UpdateStores(StoreModel model) {
            Error error = new Error();

            Store temp = db.FindStore(model.Id);
            temp.ForecastFactor = model.ForecastFactor;
            temp.Priority = model.Priority;
            temp.IsActive = model.IsActive;
            temp.RangeId = (model.RangeId == 0) ? null : model.RangeId;

            db.InsertOrUpdateStore(temp);

            return error;
        }
        
        #endregion

        #region Private methods

        private StoreModel mapToModel(Store entity) {
            return new StoreModel {
                Id = entity.Id,
                RangeId = (entity.RangeId == null) ? 0 : entity.RangeId.Value,
                Name = entity.Name,
                Address1 = entity.Address1,
                Address2 = entity.Address2,
                Suburb = entity.Suburb,
                State = entity.State,
                Postcode = entity.Postcode,
                Country = entity.Country,
                ForecastFactor = (entity.ForecastFactor == null) ? 0 : entity.ForecastFactor.Value,
                Priority = (entity.Priority == null) ? 0 : entity.Priority.Value,
                IsActive = (entity.IsActive == null) ? false : entity.IsActive.Value
            };
        }

        #endregion
    }
}
