using DotNet9Services.Service.LoggerService;
using System.Net.Http.Json;

namespace DotNet9Services.Service.HttpService
{
    /// <summary>
    /// Base class for making HTTP requests to external services.
    /// </summary>
    public abstract class HttpClientService(HttpClient httpclient, IB3Logger logger)
    {
        /// <summary>
        /// The HttpClient, only protected so other classes can add headers.
        /// </summary>
        protected readonly HttpClient _client = httpclient;
        private readonly IB3Logger _logger = logger;

        /// <summary>
        /// Create the authentication header.
        /// </summary>
        internal abstract Task CreateAuthHeaderAsync();

        /// <summary>
        /// Executes the HTTP request and handles logging and error handling.
        /// </summary>
        /// <param name="httpRequest">A function that takes a CancellationToken and returns the HTTP response.</param>
        /// <param name="url">The request URL.</param>
        /// <param name="httpMethod">The HTTP method (e.g. GET, POST).</param>
        /// <param name="cancellationToken">An optional CancellationToken.</param>
        /// <returns>An OperationResult containing the HTTP response.</returns>
        private async Task<OperationResult<HttpResponseMessage>> ExecuteHttpRequestAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> httpRequest,
            string url,
            string httpMethod,
            CancellationToken cancellationToken = default)
        {
            // Create the authentication header before executing the request.
            await CreateAuthHeaderAsync();

            try
            {
                HttpResponseMessage result = await httpRequest(cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    _logger.Information("The HTTP request ({url}) was successful with statuscode: {statuscode}.", url, result.StatusCode);
                    return new SuccessResult<HttpResponseMessage>(result);
                }
                else
                {
                    _logger.Warning("The HTTP request ({url}) was NOT successful with statuscode: {statuscode}.", url, result.StatusCode);
                    string content = await result.Content.ReadAsStringAsync();
                    return new ErrorResult<HttpResponseMessage>([$"StatusCode: {result.StatusCode}", $"Content: {content}", $"ReasonPhrase: {result.ReasonPhrase}"], result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.FatalHttpException(nameof(HttpClientService), nameof(ExecuteHttpRequestAsync), httpMethod, url, ex);
                return new ExceptionResult<HttpResponseMessage>(ex);
            }
        }

        public Task<OperationResult<HttpResponseMessage>> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(GetAsync), "GET", url);
            return ExecuteHttpRequestAsync(ct => _client.GetAsync(url, ct), url, "GET", cancellationToken);
        }

        public Task<OperationResult<HttpResponseMessage>> PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(PostAsync), "POST", url);
            return ExecuteHttpRequestAsync(ct => _client.PostAsync(url, content, ct), url, "POST", cancellationToken);
        }

        public Task<OperationResult<HttpResponseMessage>> PutAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(PutAsync), "PUT", url);
            return ExecuteHttpRequestAsync(ct => _client.PutAsync(url, content, ct), url, "PUT", cancellationToken);
        }

        public Task<OperationResult<HttpResponseMessage>> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(DeleteAsync), "DELETE", url);
            return ExecuteHttpRequestAsync(ct => _client.DeleteAsync(url, ct), url, "DELETE", cancellationToken);
        }

        public Task<OperationResult<HttpResponseMessage>> PostAsJsonAsync<T>(string url, T content, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(PostAsJsonAsync), "POST", url);
            return ExecuteHttpRequestAsync(ct => _client.PostAsJsonAsync(url, content, ct), url, "POST", cancellationToken);
        }

        public Task<OperationResult<HttpResponseMessage>> PutAsJsonAsync<T>(string url, T content, CancellationToken cancellationToken = default)
        {
            _logger.InformationHttpRequest(nameof(HttpClientService), nameof(PutAsJsonAsync), "PUT", url);
            return ExecuteHttpRequestAsync(ct => _client.PutAsJsonAsync(url, content, ct), url, "PUT", cancellationToken);
        }
    }
}
