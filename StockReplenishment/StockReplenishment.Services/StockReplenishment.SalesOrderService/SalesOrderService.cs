using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using StockReplenishment.CommonService;
using StockReplenishment.SalesOrderService;
using StockReplenishment.SqlDAL;
using StockReplenishment.RopesDAL;
using StockReplenishment.Models.Models;
using StockReplenishment.Enumerations;

namespace StockReplenishment.SalesOrderService {
    public class SalesOrderService : CommonService.CommonService {

        public SalesOrderService(StockReplenishmentEntities db, RopesDAL.RopesDAL ropesEntities = null) : base(db, ropesEntities) { }

        public Error CreateSales(List<SalesOrderItem> salesItems) {
            // On entry, a list of sale items to be added to a sale file is provided
            var error = new Error();

            try {
                if (salesItems != null && salesItems.Count() > 0) {
                    // Create a temporary file
                    //string tempFile = Path.GetTempFileName();
                    string tempFile = Path.GetTempPath() + "ErplySaleFile" + Guid.NewGuid().ToString() + ".csv";
                    Store store = null;

                    int orderNumber = db.GetNextOrderNumber();

                    using (var sw = new StreamWriter(tempFile)) {
                        sw.WriteLine("Brand,Order Number,Order Date,Ship Date Start,Ship Date End,Buyer,Customer Code,Sales Rep,Alt Sales Rep,Currency Code,Order Total,Style Name,Style Number,UPC,Color,Color Code,Size,Quantity,Price Per,Payment Terms,Shipping Line 1,Shipping Line 2,Shipping Line 3,Shipping Line 4,Shipping City,Shipping State,Shipping Zip,Shipping Country,Shipping Code,Billing Line 1,Billing Line 2,Billing Line 3,Billing Line 4,Billing City,Billing State,Billing Zip,Billing Country,Billing Code,Order Notes,Customer PO Number,Order Tags");

                        foreach (var item in salesItems) {
                            if (store == null) store = db.FindStore(item.StoreId);

                            var rc = addDelimiters(item.Brand, false);                  // Brand
                            rc += "," + orderNumber.ToString();                         // Order Number
                            rc += addDelimiters(item.SaleDate.ToString("yyyy-MM-dd"));  // Order Date
                            rc += ",";                                                  // Ship Date Start
                            rc += ",";                                                  // Ship Date End
                            rc += addDelimiters("ERPLY");                               // Buyer
                            rc += ",";                                                  // Customer Code
                            rc += ",";                                                  // Sales Rep
                            rc += ",";                                                  // Alt Sales Rep
                            rc += addDelimiters(item.CurrencyCode);                     // Currency Code
                            rc += ",";                                                  // Order Total
                            rc += addDelimiters(item.ProductName);                      // Style Name
                            rc += addDelimiters(item.ProductCode);                      // Style Number
                            rc += ",";                                                  // UPC
                            rc += ",";                                                  // Color
                            rc += ",";                                                  // Color Code
                            rc += ",";                                                  // Size
                            rc += $",{item.Quantity}";                                  // Quantity
                            rc += $",{item.Price}";                                     // Price Per
                            rc += ",";                                                  // Payment Terms
                            rc += addDelimiters(store.Name);                            // Shipping Line 1
                            rc += addDelimiters(store.Address1);                        // Shipping Line 2
                            rc += addDelimiters(store.Address2);                        // Shipping Line 3
                            rc += ",";                                                  // Shipping Line 4
                            rc += addDelimiters(store.Suburb);                          // Shipping City
                            rc += addDelimiters(store.State);                           // Shipping State
                            rc += addDelimiters(store.Postcode);                        // Shipping Zip
                            rc += addDelimiters(store.Country);                         // Shipping Country
                            rc += ",";                                                  // Shipping Code
                            rc += ",";                                                  // Billing Line 1
                            rc += ",";                                                  // Billing Line 2
                            rc += ",";                                                  // Billing Line 3
                            rc += ",";                                                  // Billing Line 4
                            rc += ",";                                                  // Billing City
                            rc += ",";                                                  // Billing State
                            rc += ",";                                                  // Billing Zip
                            rc += ",";                                                  // Billing Country
                            rc += ",";                                                  // Billing Code
                            rc += ",";                                                  // Order Notes
                            rc += ",";                                                  // Customer PO Number
                            rc += ",";                                                  // Order Tags

                            sw.WriteLine(rc);
                        }
                        sw.Close();

                        // Queue the file for sending
                        db.AddFileTransferQueue(tempFile);
                    }
                }

            } catch(Exception e) {
                error.SetError(e);
            }

            return error;
        }

        private string addDelimiters(string str, bool bAddPrefixComma = true) {
            return (bAddPrefixComma ? "," : "") + "\"" + str + "\"";
        }

        public Error SendQueuedFiles() {
            var error = new Error();

            try {
                string pickupFolder = GetConfigurationSetting("RopesPickupFolder", "");

                var fileList = db.FindTransferQueues().ToList();

                LogService.WriteLog(TransType.Replenishment, null, null, $"Moving {fileList.Count()} file(s) to Ropes pickup folder: " + pickupFolder);

                foreach (var file in fileList) {
                    var tempFile = file.FileName;
                    int pos = tempFile.LastIndexOf("\\");
                    string fileName = tempFile.Substring(pos + 1);

                    string targetFile = pickupFolder + "\\" + fileName;

                    LogService.WriteLog(TransType.Replenishment, null, null, $"Moving {tempFile} to {targetFile}");

                    File.Move(tempFile, targetFile);

                    // Now rename it so ROPES picks it up
                    pos = targetFile.LastIndexOf(".");
                    string renameFile = targetFile.Substring(0, pos) + ".csv";

                    LogService.WriteLog(TransType.Replenishment, null, null, $"Renaming {targetFile} to {renameFile}");

                    File.Move(targetFile, renameFile);

                    db.DeleteFileTransferQueue(file);
                }

            } catch(Exception e) {
                error.SetError(e);
            }

            return error;
        }
    }
}
