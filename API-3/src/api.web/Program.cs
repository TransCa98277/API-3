using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace api.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var logger = NLog.LogManager.Setup().RegisterNLogWeb().GetCurrentClassLogger();
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                logger.Debug("init main");
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {
                // NLog: catch setup errors
                logger.Error("Request failed, Application Stopped... -> {0}.{1} Error {2}", method.ReflectedType.Name, method.Name, e.ToString());
                throw;
            }
            finally
            {
                logger.Debug("Application Stopping... -> {0}.{1}", method.ReflectedType.Name, method.Name);
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    })
                    .UseNLog()
                    //.UseIISIntegration()
                    .UseUrls("http://localhost:5004")
                    .Build();
    }
}
