using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;

namespace TSB100UserProfileService.App_Code
{
    public class StartUp
    {
        // Denna metod kommer att köras vid startup
        public static void AppInitialize()
        {
            // Create a Logger-singleton     
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(@"C:\logs\WcfLog-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // It's good to know when the application started (for statistics an such) Let's log that:
            Log.Information("TSB100UserProfileService: Application started\n-----------------------------------------");

            // Här försöker vi skriva till logfilen med lite olika nivåer
            // Verbose är lägsta möjliga nivå, men eftersom vi satte
            // .MinimumLevel.Debug() här ovan så ignoreras alla Log.Verbose()
            //Log.Verbose("Demo - Verbose");
            //Log.Debug("Demo - Debug");
            //Log.Information("Demo - Information");
            //Log.Warning("Demo - Warning");
            //Log.Error("Demo - Error");
            //Log.Fatal("Demo - Fatal");

            // FileReader is used for reading log files
            FileReader.MakeSingletonReader();
        }
    }
}