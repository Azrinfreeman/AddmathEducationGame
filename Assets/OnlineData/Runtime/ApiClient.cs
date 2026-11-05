using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MathFun.OnlineData
{
    /// <summary>What came back from one request.</summary>
    public sealed class ApiResult
    {
        public long Status;                 // HTTP status, 0 when the server could not be reached
        public string Body = "";
        public bool NetworkFailure;         // no connection / timeout / DNS problem
        public int RetryAfterSeconds;       // from the Retry-After header, 0 if absent

        public bool IsSuccess { get { return !NetworkFailure && Status >= 200 && Status < 300; } }

        /// <summary>True for problems that may go away by themselves: retry later.</summary>
        public bool ShouldRetryLater { get { return NetworkFailure || Status == 429 || Status >= 500; } }

        public string ErrorCode
        {
            get
            {
                ErrorEnvelope envelope = Parse<ErrorEnvelope>();
                return envelope != null && envelope.error != null ? envelope.error.code : "";
            }
        }

        public T Parse<T>() where T : class
        {
            try { return JsonUtility.FromJson<T>(Body); }
            catch { return null; }
        }
    }

    /// <summary>Sends HTTPS requests to the PHP API. It knows nothing about players or saves.</summary>
    public sealed class ApiClient
    {
        private readonly ApiConfiguration configuration;

        public ApiClient(ApiConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool IsEnabled { get { return configuration.onlineEnabled; } }

        /// <param name="method">GET, POST, PUT, PATCH or DELETE</param>
        /// <param name="route">for example /v1/players</param>
        /// <param name="jsonBody">null when there is no body</param>
        /// <param name="token">the sign-in token, or null</param>
        public Task<ApiResult> SendAsync(string method, string route, string jsonBody, string token)
        {
            TaskCompletionSource<ApiResult> completion = new TaskCompletionSource<ApiResult>();
            if (!configuration.onlineEnabled)
            {
                completion.SetResult(new ApiResult { NetworkFailure = true });
                return completion.Task;
            }

            UnityWebRequest request = new UnityWebRequest(configuration.BuildUrl(route), method);
            request.timeout = configuration.timeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            if (jsonBody != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
                // Some shared hosts remove Authorization; the server also accepts this header.
                request.SetRequestHeader("X-Auth-Token", token);
            }

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                ApiResult result = new ApiResult
                {
                    Status = request.responseCode,
                    Body = request.downloadHandler != null ? request.downloadHandler.text : "",
                    NetworkFailure = request.result == UnityWebRequest.Result.ConnectionError,
                };
                int.TryParse(request.GetResponseHeader("Retry-After"), out result.RetryAfterSeconds);
                request.Dispose();
                completion.TrySetResult(result);
            };
            return completion.Task;
        }
    }
}
