using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.ProductService {
    public partial class ProductService {

        public List<RangeModel> GetRanges() {
            List<RangeModel> model = new List<RangeModel>();

            var allItems = db.FindRanges();

            foreach(var item in allItems) {
                model.Add(mapToModel(item));
            }

            return model;
        }

        public RangeModel GetRange(int id) {
            Range range = db.FindRange(id);
            return mapToModel(range);
        }

        public Error InsertOrUpdateRange(RangeModel model) {
            Error error = new Error();

            Range temp = null;
            if(model.Id != 0) temp = db.FindRange(model.Id);
            if (temp == null) temp = new Range();

            temp.Name = model.Name;
            temp.IsActive = model.IsActive;

            db.InsertOrUpdateRange(temp);

            return error;
        }

        public Error DeleteRange(RangeModel model) {
            Error error = new Error();
            db.DeleteRange(model.Id);
            return error;
        }

        private RangeModel mapToModel(Range entity) {
            return new RangeModel {
                Id = entity.Id,
                Name = entity.Name,
                IsActive = entity.IsActive
            };
        }
    }
}
