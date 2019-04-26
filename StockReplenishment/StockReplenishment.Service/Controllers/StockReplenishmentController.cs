using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using StockReplenishment.ErplyService;
using StockReplenishment.Models.Models;
using StockReplenishment.Service.Controllers;
using StockReplenishment.Enumerations;

namespace StockReplenishment.Service {
    public class StockReplenishmentController : BaseController {

        public StockReplenishmentController() { }

        public void Run(string[] args) {
            string msg = "";

            bool bRetry = true,
                 bGetPurchases = (args[0].ToUpper() == "/GETPURCHASES" ? true : false);
            //bool bCreateOrders = (args[0].ToUpper() == "/CREATEORDERS" ? true : false);
            int warehouseId = (args.Count() > 1 ? Convert.ToInt32(args[1]) : -1);

            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            DateTimeOffset dateLastRun = (args.Count() > 2 ? DateTimeOffset.Parse(args[2]) : today);

            var error = new Error();
            EAPI api = new EAPI();
            int unixRunTimeStamp;

            while (bRetry) {
                bRetry = false;

                try {
                    // Startup
                    LogService.WriteLog(TransType.Application, null, null, "Started with parameter: " + args[0]);

                    switch (args[0].ToUpper()) {
                    case "/GETPURCHASES":
                        // Get Erply Orders and save to TEMP table
                        LogService.WriteLog(TransType.Erply, null, null, "Processing ErplyOrders...");
                        unixRunTimeStamp = ErplyService.ProcessErply(api);

                        LogService.WriteLog(TransType.Application, null, null, $"UnixTimeStamp: {unixRunTimeStamp}");

                        // Update the product database fom Erply
                        error = ProductService.UpdateProducts(api, unixRunTimeStamp);

                         if (!error.IsError) {
                            // Update Stock on Hand
                            error = ErplyService.GetStockOnHandForAllStores(api, unixRunTimeStamp, warehouseId);

                            if (!error.IsError) {
                                // Update Stock in transit
                                error = ErplyService.GetStockInTransit(api, unixRunTimeStamp, warehouseId);
                            }
                        }
                        break;

                    case "/CREATEORDERS":
                        // Get the last order creation time
                        unixRunTimeStamp = db.GetLastOrderCreationTime();
                        db.UpdateLastOrderCreationTime(DateTimeOffset.Now);

                        // Get TEMP table data and save into ProductsSold table
                        LogService.WriteLog(TransType.Replenishment, null, null, "Saving TEMP data to ProductsSold table...");
                        ErplyService.SaveTempToProductsSoldTable(unixRunTimeStamp);

                        LogService.WriteLog(TransType.Replenishment, null, null, "Cleaning up the TEMP table...");
                        ErplyService.CleanTempTable();

                        if (!error.IsError) {
                            // Create the replenishment orders
                            error = ReplenishmentCalculatorService.CreateOrders(unixRunTimeStamp);

                            LogService.CleanupLogs();
                        }
                        db.UpdateLastOrderCreationTime(DateTimeOffset.Now);
                        break;

                    case "/GETSTOCKINTRANSIT":
                        // GETSTOCKINTRANSIT warehouseid|0 date
                        unixRunTimeStamp = (int)dateLastRun.ToUnixTimeSeconds();
                        error = ErplyService.GetStockInTransit(api, unixRunTimeStamp, warehouseId);
                        if(error.IsError) bRetry = true;
                        break;

                    case "/GETSTOCKONHAND":
                        // GETSTOCKONHAND warehouseid|0 date
                        unixRunTimeStamp = (int)dateLastRun.ToUnixTimeSeconds();
                        error = ErplyService.GetStockOnHandForAllStores(api, unixRunTimeStamp, warehouseId);
                        if (error.IsError) bRetry = true;
                        break;
                    }
                    if (error.IsError) {
                        msg = error.Message;
                        LogService.WriteLog(TransType.Application, null, null, msg);

                    } else {
                        LogService.WriteLog(TransType.Application, null, null, "Done.");
                    }

                } catch (Exception e1) {
                    msg = "Error: " + e1.Message;
                    LogService.WriteLog(TransType.Application, null, null, msg);
                }

                if(bRetry) {
                    // Wait until the next hour tick over
                    var tempNow = DateTime.Now;
                    var tempOneHour = tempNow.AddHours(1);
                    var waitUntil = new DateTime(tempOneHour.Year, tempOneHour.Month, tempOneHour.Day, tempOneHour.Hour, 2, 0);

                    LogService.WriteLog(TransType.Application, null, null, $"Waiting for retry at: " + waitUntil.ToString());

                    while (tempNow < waitUntil) {
                        Thread.Sleep(5000);
                        tempNow = DateTime.Now;
                    }

                } else {
                    LogService.WriteLog(TransType.Application, null, null, "");
                }
            }
        }
    }
}
