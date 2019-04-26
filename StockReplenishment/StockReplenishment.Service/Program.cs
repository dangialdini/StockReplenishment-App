using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Service {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            if (args.Length < 1 ||
                (args[0] != "/GETPURCHASES" &&
                 args[0] != "/CREATEORDERS" &&
                 args[0] != "/GETSTOCKINTRANSIT" &&
                 args[0] != "/GETSTOCKONHAND")) {

                Console.WriteLine("Error: Invalid parameters");
                Console.WriteLine("Usage: StockReplenishment.Service.exe /GETPURCHASES|/CREATEORDERS|/GETSTOCKINSTRANSIT|/GETSTOCKONHAND [WarehouseId] [fromdate]");

            } else {
                var app = new StockReplenishmentController();
                app.Run(args);
            }
        }
    }
}
