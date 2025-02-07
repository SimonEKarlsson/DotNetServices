using Serilog.Events;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ILogger = Serilog.ILogger;

namespace DotNet9Services.Service.LoggerService
{
    public class B3Logger(ILogger logger) : IB3Logger
    {
        private readonly ILogger _logger = logger;

        public void FatalHttpException(string className, string methodName, string httpMethod, string url, Exception exception)
        {
            StackTrace stackTrace = new(exception, true);
            StackFrame? frame = stackTrace.GetFrame(0);

            string fileName = frame?.GetFileName() ?? "[Unknown filename]";
            string line = frame?.GetFileLineNumber().ToString() ?? "[Unknown line]";
            string method = GetMethodName(frame?.GetMethod());
            string exceptionType = exception.GetType().Name;
            string traceCall = GetStackTrace(stackTrace);

            string exceptionString = $"{method} in {fileName} threw an {exceptionType} on line {line}.\nMessage:\n{exception.Message}\nInnerException:\n{AppendInnerExceptions(exception)}\nStackCall:\n{traceCall}";

            _logger.Fatal(exception, "{Class}:{Method}: Failed making a {httpMethod} HTTP request to endpoint: {url}.\nMessage:\n{message}", className, methodName, httpMethod, url, exceptionString);
        }

        /// <summary>
        /// Logs an informational message when an HTTP request is initiated.
        /// </summary>
        /// <param name="className">The name of the class initiating the request.</param>
        /// <param name="methodName">The name of the method initiating the request.</param>
        /// <param name="httpMethod">The HTTP method used for the request.</param>
        /// <param name="url">The URL to which the request is made.</param>
        public void InformationHttpRequest(string className, string methodName, string httpMethod, string url)
        {
            _logger.Information("{Class}:{Method}: Making a {httpMethod} HTTP request to endpoint: {url}.", className, methodName, httpMethod, url);
        }

        /// <summary>
        /// Write an event to the log.
        /// </summary>
        /// <param name="logEvent">The event to write.</param>
        public void Write(LogEvent logEvent)
        {
            _logger.Write(logEvent);
        }

        /// <summary>
        /// Gets the method name and its parameters from a <see cref="MethodBase"/> object.
        /// </summary>
        /// <param name="method">The method from which to retrieve the name and parameters.</param>
        /// <returns>A string representing the method name and parameters.</returns>
        private static string GetMethodName(MethodBase? method)
        {
            if (method == null)
            {
                return "[Unknown method]";
            }

            ParameterInfo[] parameters = method.GetParameters();
            string parametersString = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
            return $"{method.Name}({parametersString})";
        }

        /// <summary>
        /// Retrieves a formatted stack trace from the provided <see cref="StackTrace"/> object.
        /// </summary>
        /// <param name="stackTrace">The stack trace from which to extract details.</param>
        /// <returns>A formatted string representing the stack trace.</returns>
        private static string GetStackTrace(StackTrace stackTrace)
        {
            StringBuilder sb = new();
            System.Diagnostics.StackFrame[] frames = stackTrace.GetFrames();

            if (frames != null)
            {
                foreach (System.Diagnostics.StackFrame frame in frames)
                {
                    if (frame.GetFileName() != null)
                    {
                        sb.AppendLine($"{frame.GetFileName()} | {GetMethodName(frame.GetMethod())}");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Recursively appends messages from inner exceptions.
        /// </summary>
        /// <param name="exception">The exception containing inner exceptions.</param>
        /// <returns>A formatted string of inner exception messages.</returns>
        private static string AppendInnerExceptions(Exception? exception)
        {
            StringBuilder sb = new();
            int level = 1;
            while (exception != null)
            {
                sb.AppendLine($"Inner Exception Level {level}: {exception.Message}");
                exception = exception.InnerException;
                level++;
            }
            return sb.ToString();
        }
    }
}
