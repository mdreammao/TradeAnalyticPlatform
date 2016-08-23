using BackTestingPlatform.Utilities;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Core
{
    public class MyNLogConfig
    {
        public static void Apply()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();
            var rootDir = ConfigurationManager.AppSettings["Log.RootPath"];            
            var layout0 = @"${date:format=yyyy-MM-dd HH\:mm\:ss} [${pad:padding=5:inner=${level:uppercase=true}}] ${logger:shortName=true}: ${message}";
            var layout1 = @"${date:format=yyyy-MM-dd HH\:mm\:ss} [${pad:padding=5:inner=${level:uppercase=true}}] ${logger}: ${message}";
            if (rootDir == null) rootDir = "${basedir}";
            // Step 2. Create targets and add them to the configuration 
            var con = new ColoredConsoleTarget();   
            var f1 = new FileTarget();
            var f2 = new FileTarget();
            config.AddTarget("console", con);
            config.AddTarget("f1", f1);
            config.AddTarget("f2", f2);

            // Step 3. Set target properties 
            
            con.Layout = layout0;
            f1.FileName = rootDir + "/all.${shortdate}.log";
            f1.Layout = layout1;
            f2.FileName = rootDir + "/error.${shortdate}.log";
            f2.Layout = layout1;

            // Step 4. Define rules            
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, con));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, f1));           
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, f2));


            // Step 5. Activate the configuration
            LogManager.Configuration = config;

           
        }
    }
}
