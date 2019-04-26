using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.SqlDAL;
using StockReplenishment.Enumerations;

namespace StockReplenishment.LogService
{
    public partial class LogService {

        public string LogFolder = "";

        private StockReplenishmentEntities db = new StockReplenishmentEntities();

        #region Contruction

        public LogService() {
            try {
                LogFolder = ConfigurationManager.AppSettings["LogFolderLocation"];
            } catch { }

            if (!Directory.Exists(LogFolder)) {
                Directory.CreateDirectory(LogFolder);
            }
        }

        #endregion

        #region Public methods

        public void WriteLog(TransType transType, int?storeId, int? productId, string message) {
            string extra = (message.IndexOf("error code: 1002") == -1 ? "" : " - hourly request limit exceeded");

            var log = new Log { LogDateTime = DateTimeOffset.Now, TransType = (int)transType, StoreId = storeId, ProductId = productId, Message = message };
            db.InsertOrUpdateLog(log);

            string msg = log.LogDateTime.ToString("dd/MM/yyyy HH:mm:ss") + " " + message + extra;
            Console.WriteLine(msg);

            using (StreamWriter sw = new StreamWriter(GetLogFile(DateTimeOffset.Now), true)) {
                sw.WriteLine(msg);
            }
        }

        public string GetLogFile(DateTimeOffset dt) {
            return LogFolder + "\\" + dt.ToString("yyyyMMdd") + ".log";
        }

        public void CleanupLogs(int logKeepDays = -1) {
            try {
                if(logKeepDays == -1) {
                    try {
                        logKeepDays = Convert.ToInt32(ConfigurationManager.AppSettings["LogKeepDays"]);
                    } catch { }

                }

                db.CleanLog(DateTime.Now.AddDays(logKeepDays * -1));

                var logList = Directory.GetFiles(LogFolder, "*.log");
                if(logList != null && logList.Length > logKeepDays) {
                    int deleteTo = logList.Length - logKeepDays;

                    for(int i = 0; i < deleteTo; i++) {
                        try {
                            File.Delete(logList[i]);
                        } catch { }
                    }
                }

            } catch {}
        }

        #endregion
    }
}
