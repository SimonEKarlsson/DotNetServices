using DotNet9Services.Service.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using ILogger = Serilog.ILogger;

namespace DotNet9Services.Service.LoggerService
{
    public interface IB3Logger : ILogger
    {
        /// <summary>
        /// Logs an informational message when an HTTP request is initiated.
        /// </summary>
        /// <param name="className">The name of the class initiating the request.</param>
        /// <param name="methodName">The name of the method initiating the request.</param>
        /// <param name="httpMethod">The HTTP method used for the request.</param>
        /// <param name="url">The URL to which the request is made.</param>
        void InformationHttpRequest(string className, string methodName, string httpMethod, string url);
        void FatalHttpException(string className, string methodName, string httpMethod, string url, Exception exception);
    }

    public static class AddLoggerService
    {
        public static void B3Logger(this WebApplicationBuilder builder, Action<B3LoggerConfiguration>? configureLogger = null)
        {
            try
            {
                // Create a default configuration instance.
                B3LoggerConfiguration config = new();
                // Allow the caller to customize the configuration via the lambda.
                configureLogger?.Invoke(config);

                bool isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                string correlationHeaderName = "correlation-id";

                // Use the customized values if provided, otherwise fallback to configuration values.
                string aiConnectionString = config.AIConnectionString ?? builder.Configuration.B3ThrowIfNullOrEmpty("ApplicationInsightsCs");
                string dbConnectionString = config.DBConnectionString ?? builder.Configuration.B3ThrowIfNullOrEmpty("SqlConnectionStringsLog");
                string dbTableName = config.DBTableName ?? "Log";

                //options for SQL logging
                MSSqlServerSinkOptions sinkOptions = new()
                {
                    TableName = dbTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 50,
                    BatchPeriod = TimeSpan.FromSeconds(5)
                };

                //options for application insights logging
                TelemetryConfiguration telemetryConfiguration = new()
                {
                    ConnectionString = aiConnectionString,
                    TelemetryChannel = new ServerTelemetryChannel()
                };

                //using Serilogs ILogger. It has better perfomance than Microsofts ILogger
                ILogger logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .MinimumLevel.Is(LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .WriteTo.Async(a => a.MSSqlServer(connectionString: dbConnectionString, sinkOptions: sinkOptions))
                    .WriteTo.Console()
                    .WriteTo.Async(a => a.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces))
                    .Enrich.FromLogContext()
                    .Enrich.WithCorrelationId(headerName: correlationHeaderName, addValueIfHeaderAbsence: true)
                    .CreateLogger();

                builder.Host.UseSerilog(logger);
                builder.Services.AddSingleton<IB3Logger, B3Logger>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n");
                Console.WriteLine($"Problem with initialize logger.\r\n{ex.Message}");
                Console.WriteLine("\r\n");
                throw;
            }
        }
    }

    public class B3LoggerConfiguration
    {
        /// <summary>
        /// Connectionstring to Application Insights
        /// </summary>
        public string? AIConnectionString { get; set; }

        /// <summary>
        /// Connectionstring to SQL database
        /// </summary>
        public string? DBConnectionString { get; set; }

        /// <summary>
        /// The name of SQL Table
        /// </summary>
        public string? DBTableName { get; set; }
    }
}
