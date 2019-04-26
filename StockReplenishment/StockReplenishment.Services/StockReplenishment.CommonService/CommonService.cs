using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Models.Models;
using StockReplenishment.SqlDAL;
using StockReplenishment.RopesDAL;

namespace StockReplenishment.CommonService
{
    public partial class CommonService {

        #region Private members

        #endregion

        #region Protected members

        protected StockReplenishmentEntities db;
        protected RopesDAL.RopesDAL ropesDb;

        protected LogService.LogService _logService = null;
        protected LogService.LogService LogService {
            get {
                if (_logService == null) _logService = new LogService.LogService();
                return _logService;
            }
        }

        #endregion

        #region Construction

        public CommonService(StockReplenishmentEntities dbEntities, RopesDAL.RopesDAL ropesEntities = null) {
            db = dbEntities;
            ropesDb = ropesEntities;
        }

        #endregion

        #region Validation

        protected Error isValidRequiredString(string str, int maxLength, string fieldName, string errorMsg) {
            var error = new Error();
            if (string.IsNullOrEmpty(str) || str.Length > maxLength) {
                error.SetError(errorMsg, fieldName, maxLength.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidNonRequiredString(string str, int maxLenth, string fieldName, string errorMsg) {
            Error error = new Error();
            if(!string.IsNullOrEmpty(str) && str.Length > maxLenth) {
                error.SetError(errorMsg, fieldName, maxLenth.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidRequiredInt(int? intValue, string fieldName, string errorMsg) {
            var error = new Error();
            if (intValue == null || intValue == 0) {
                error.SetError(errorMsg, fieldName, fieldName);
            }
            return error;
        }

        protected Error isValidRequiredDate(DateTimeOffset? dateValue, string fieldName, string errorMsg) {
            var error = new Error();
            if (dateValue == null) {
                error.SetError(errorMsg, fieldName, fieldName);
            }
            return error;
        }
        
        #endregion

        #region Configuration file settings

        protected string GetConfigurationSetting(string key, string defaultValue) {
            string result = defaultValue;
            try {
                result = ConfigurationManager.AppSettings[key];
            } catch { }
            return result;
        }

        protected int GetConfigurationSetting(string key, int defaultValue) {
            try {
                return Convert.ToInt32(ConfigurationManager.AppSettings[key]);
            } catch {
                return defaultValue;
            }
        }

        protected bool GetConfigurationSetting(string key, bool defaultValue) {
            string temp = GetConfigurationSetting(key, defaultValue.ToString()).ToLower();
            return (temp == "true" || temp == "yes" || temp == "1");
        }

        #endregion

        #region Error handling

        protected Error GetError(string errorCode, string responseStatus) {
            var error = new Error();
            if (errorCode != "0") {
                if (errorCode == "1002") {
                    error.SetError($"Verify user failure: error code: {errorCode} - hourly request limit exceeded");
                } else {
                    error.SetError($"Erply Error: {errorCode}: {responseStatus}");
                }
            }
            return error;
        }

        #endregion
    }
}
