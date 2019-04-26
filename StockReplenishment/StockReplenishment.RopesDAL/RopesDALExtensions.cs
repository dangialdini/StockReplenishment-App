using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Models.Models;

namespace StockReplenishment.RopesDAL {
    public partial class RopesDAL {

        #region Product table

        OleDbDataReader rs = null;

        public ProductModel FindProducts() {
            if(rs == null) rs = findRecords("tblItems", "");
            var model = mapToProductModel(rs);
            if(model == null) {
                if(rs != null) rs.Close();
                rs = null;
            }
            return model;
        }

        public ProductModel FindProduct(int id) {
            var rs = findRecords("tblItems", $"ItemID={id}");
            var model = mapToProductModel(rs);
            rs.Close();
            return model;
        }

        public ProductModel FindProduct(string productCode) {
            var rs = findRecords("tblItems", $"ItemNumber='{productCode}'");
            var model = mapToProductModel(rs);
            rs.Close();
            return model;
        }

        private ProductModel mapToProductModel(OleDbDataReader rs) {
            if(rs == null || !rs.Read()) {
                return null;
            } else {
                var model = new ProductModel();
                model.Id = Convert.ToInt32(rs["ItemID"]);
                model.ProductCode = rs["ItemNumber"].ToString();
                model.ProductName = rs["ItemName"].ToString();
                model.MSQ = Convert.ToInt32(rs["CustomField3"]);
                return model;
            }
        }
        
        #endregion
    }
}
