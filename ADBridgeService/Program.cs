using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ADBridgeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var startupLogger = AppLoggerConfiguration.NewStartupLogger();

            try
            {
                LogBeginBuildingHost(startupLogger);
                CreateHostBuilder(args, startupLogger).Build().Run();
            }
            catch (Exception ex)
            {
                var fatalLogger = Log.Logger;
                // если логгер не удалось инициализировать, то используем стартовый логгер
                if (fatalLogger == null)
                {
                    fatalLogger = startupLogger;
                }

                LogFatalHostFailure(fatalLogger, ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, Serilog.ILogger startupLogger) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddEnvironmentVariables(prefix: "ADBRIDGE_");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    AppLoggerConfiguration.SetupAppLogger(hostingContext, loggerConfiguration, startupLogger);
                });

        private static void LogBeginBuildingHost(Serilog.ILogger startupLogger)
        {
            startupLogger.Information("ADBridge. Запуск сервиса моста в AD");
        }

        private static void LogFatalHostFailure(Serilog.ILogger fatalLogger, Exception ex)
        {
            fatalLogger.Fatal(
                ex,
                "ADBridge. Неожиданная ошибка, прерывание работы сервиса"
            );
        }
    }
}
