using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.Async;
using Microsoft.Extensions.Configuration;
using System.IO;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace ADBridgeService
{
    public class AppLoggerConfiguration
    {
        internal static ILogger NewStartupLogger()
        {
            // стартовый логгер выводит на консоль и в системный журнал
            // TODO: если сервис, не писать в консоль
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", GetApplicationName())
                // здесь в системный журнал пишем все, и синхронно
                .WriteTo.EventLog("ADBridge", manageEventSource: false)
            ;

            if (!IsWindowsService())
            {
                loggerConfiguration.WriteTo.Console();
            }

            return loggerConfiguration.CreateLogger();
        }

        internal static void SetupAppLogger(
            HostBuilderContext hostingContext, 
            LoggerConfiguration loggerConfiguration,
            ILogger startupLogger
        )
        {
            var appConfiguration = hostingContext.Configuration;
            var logsPath = GetLogsPath(appConfiguration, startupLogger);
            var flushLogsOnWrite = GetFlushLogsOnWrite(appConfiguration);

            loggerConfiguration
                .MinimumLevel.Debug()
                // для стандартных логов не даем шуметь
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                // TODO: добавить версию
                .Enrich.WithProperty("ApplicationName", GetApplicationName())
                .Enrich.WithThreadId()
                // в системный журнал пишем асинхронно, он медленный
                // в системный журнал идут только ошибки
                .WriteTo
                    .Async(l => l.EventLog("ADBridge", manageEventSource: false, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error))
                .WriteTo
                    .File(
                        formatter: new RenderedCompactJsonFormatter(),
                        path: Path.Combine(logsPath, "adbridge_log-.ndjson"),
                        buffered: !flushLogsOnWrite,
                        rollingInterval: RollingInterval.Day,
                        // не сносим логи автоматически
                        retainedFileCountLimit: null
                    )
                ;

            if (!IsWindowsService())
            {
                loggerConfiguration.WriteTo.Console();
            }
        }

        private static bool IsWindowsService()
        {
            return WindowsServiceHelpers.IsWindowsService();
        }

        private static string GetApplicationName()
        {
            return typeof(Program).Assembly.GetName().Name;
        }

        private static bool GetFlushLogsOnWrite(IConfiguration appConfiguration)
        {
            return appConfiguration.GetValue<bool>("FlushFileLogsOnWrite");
        }

        private static string GetLogsPath(IConfiguration appConfiguration, ILogger startupLogger)
        {
            var logsPath = appConfiguration.GetValue<string>("LogsPath");
            if (string.IsNullOrEmpty(logsPath))
            {
                LogPathToLogsNotProvided(startupLogger);
                return GetLogsPathInAppDirectory(startupLogger);
            }

            if (!Directory.Exists(logsPath))
            {
                LogPathToLogsNotFound(startupLogger, logsPath);
            }

            return logsPath;
        }

        private static void LogPathToLogsNotFound(ILogger startupLogger, string logsPath)
        {
            startupLogger.Warning(
                "ADBridgeService. Не найден путь к папке журнала {logPath}. Папка будет создана автоматически, с правами учетной записи приложения",
                logsPath
            );
        }

        private static string GetLogsPathInAppDirectory(ILogger startupLogger)
        {
            var appLogsPath = Path.Combine(AppContext.BaseDirectory, "logs");
            LogAppLogsPath(startupLogger, appLogsPath);

            return appLogsPath;
        }

        private static void LogPathToLogsNotProvided(ILogger startupLogger)
        {
            startupLogger.Warning("ADBrideService. Не задана настройка LogsPath для пути к файлам журнала. Логи будут созданы в папке приложения");
        }

        private static void LogAppLogsPath(ILogger startupLogger, string appLogsPath)
        {
            startupLogger.Information("ADBridgeService. Логи находятся в папке приложения {appLogsPath}", appLogsPath);
        }
    }
}
