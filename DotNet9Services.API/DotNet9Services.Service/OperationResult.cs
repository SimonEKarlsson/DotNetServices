using System.Net;

namespace DotNet9Services.Service
{
    /// <summary>
    /// An abstract base class representing the response from a method, encapsulating status information, messages, and an optional value.
    /// Use <see cref="IsSuccessful"/> to determine if the operation was successful.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value returned by the method. If the method does not return a value, set <typeparamref name="T"/> to a suitable placeholder type.
    /// </typeparam>
    /// <param name="statusCode">The HTTP status code representing the result of the method.</param>
    /// <param name="messages">A list of messages providing additional information about the result.</param>
    /// <param name="value">The value returned by the method, or <c>null</c> if none.</param>
    /// <param name="isSuccessful">Indicates whether the method was successful.</param>
    public abstract class OperationResult<T>(HttpStatusCode statusCode, List<string> messages, T? value, bool isSuccessful)
    {
        /// <summary>
        /// Gets the HTTP status code representing the result of the method.
        /// </summary>
        /// <remarks>
        /// For example, set to <see cref="HttpStatusCode.BadRequest"/> if a parameter is missing, or <see cref="HttpStatusCode.InternalServerError"/> if an exception was thrown.
        /// </remarks>
        public HttpStatusCode StatusCode { get; } = statusCode;
        /// <summary>
        /// Gets a list of messages providing additional information about the result, useful for logging or debugging.
        /// </summary>
        public List<string> Messages { get; } = messages;
        /// <summary>
        /// Gets the value returned by the method. This is <c>null</c> if the method did not return a value or if the response represents an error.
        /// </summary>
        public T? Value { get; } = value;
        /// <summary>
        /// Gets a value indicating whether the method was successful.
        /// </summary>
        public bool IsSuccessful { get; } = isSuccessful;
        /// <summary>
        /// Gets a concatenated string of all messages in <see cref="Messages"/>, separated by line breaks.
        /// </summary>
        public string StringMessages => string.Join("\r\n", Messages);
        /// <summary>
        /// Gets or sets the exception associated with an error response. This is set if the response represents an error.
        /// </summary>
        /// <remarks>
        /// This property is populated when the <see cref="OperationResult{T}"/> is an instance of <see cref="ErrorResult{T}"/>.
        /// </remarks>
        public Exception? Exception { get; internal set; }
    }

    /// <summary>
    /// Represents a successful response from a method that returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the method.</typeparam>
    public class SuccessResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessResult{T}"/> class with the specified success messages and value.
        /// </summary>
        /// <param name="successMessages">A list of messages providing additional information about the successful operation.</param>
        /// <param name="value">The value returned by the method.</param>
        public SuccessResult(List<string> successMessages, T value) : base(HttpStatusCode.OK, successMessages, value, true) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessResult{T}"/> class with the specified value.
        /// The messages are set to "Ok".
        /// </summary>
        /// <param name="value">The value returned by the method.</param>
        public SuccessResult(T value) : base(HttpStatusCode.OK, ["Ok"], value, true) { }

    }

    /// <summary>
    /// Represents a successful response from a method that does not return a value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value that would be returned by the method. This should be a suitable placeholder type.
    /// </typeparam>
    public class NoContentResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoContentResult{T}"/> class with the specified success messages.
        /// </summary>
        /// <param name="successMessages">A list of messages providing additional information about the successful operation.</param>
        public NoContentResult(List<string> successMessages) : base(HttpStatusCode.NoContent, successMessages, default, true) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NoContentResult{T}"/> class.
        /// The messages are set to "Ok".
        /// </summary>
        public NoContentResult() : base(HttpStatusCode.NoContent, ["Ok"], default, true) { }
    }

    /// <summary>
    /// Represents an error response from a method.
    /// </summary>
    /// <typeparam name="T">The type of the value that would be returned by the method.</typeparam>
    public class ErrorResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResult{T}"/> class with the specified error messages and status code.
        /// </summary>
        /// <param name="errorMessages">A list of messages describing the error.</param>
        /// <param name="statusCode">The HTTP status code representing the error.</param>
        /// <remarks>
        /// The <see cref="OperationResult{T}.IsSuccessful"/> property is set to <c>false</c>, and the <see cref="OperationResult{T}.Value"/> is set to <c>null</c>.
        /// The <see cref="Exception"/> property is populated with an exception containing the error messages.
        /// </remarks>
        public ErrorResult(List<string> errorMessages, HttpStatusCode statusCode) : base(statusCode, errorMessages, default, false)
        {
            Exception = new(StringMessages);
        }
    }

    /// <summary>
    /// Represents an ExceptionError response form a method
    /// </summary>
    /// <typeparam name="T">The type of the value that would be returned by the method.</typeparam>
    public class ExceptionResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionResult{T}"/> class with the Exception from the method.
        /// </summary>
        /// <param name="ex">the <see cref="Exception"/> that was thrown.</param>
        /// <remarks>
        /// The <see cref="OperationResult{T}.IsSuccessful"/> property is set to <c>false</c> and the <see cref="OperationResult{T}.Value"/> is set to <c>null</c>.
        /// The <see cref="Exception"/> property is populated with the exception that was thrown.
        /// </remarks>
        public ExceptionResult(Exception ex) : base(HttpStatusCode.InternalServerError, [ex.Message], default, false)
        {
            Exception = ex;
        }
    }
}
