using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.SqlDAL;

namespace StockReplenishment.Service.Controllers {
    public class BaseController {

        #region Private members

        private LogService.LogService _logService = null;
        private ErplyService.ErplyService _erplyService = null;
        private ProductService.ProductService _productService = null;
        private ReplenishmentCalculatorService.ReplenishmentCalculatorService _calcService = null;

        #endregion

        #region Protected Members

        protected StockReplenishmentEntities db = new StockReplenishmentEntities();
        protected RopesDAL.RopesDAL ropesDb = new RopesDAL.RopesDAL();

        protected LogService.LogService LogService {
            get{
                if (_logService == null) _logService = new LogService.LogService();
                return _logService;
            }
        }

        protected ErplyService.ErplyService ErplyService {
            get {
                if (_erplyService == null) _erplyService = new ErplyService.ErplyService(db, ropesDb);
                return _erplyService;
            }
        }

        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db, ropesDb);
                return _productService;
            }
        }

        protected ReplenishmentCalculatorService.ReplenishmentCalculatorService ReplenishmentCalculatorService {
            get {
                if (_calcService == null) _calcService = new ReplenishmentCalculatorService.ReplenishmentCalculatorService(db, ropesDb);
                return _calcService;
            }
        }

        #endregion
    }
}
