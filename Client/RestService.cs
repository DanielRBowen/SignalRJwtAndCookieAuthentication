using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class RestService : IDisposable
    {
        private HttpClient _httpClient;
        public HttpClient HttpClient => _httpClient;

        public RestService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var acceptHeaders = httpClient.DefaultRequestHeaders.Accept;
            acceptHeaders.Clear();
            acceptHeaders.Add(new MediaTypeWithQualityHeaderValue(MediaType.Json));
        }

        public RestService(Uri baseAddress) :
            this(new HttpClient { BaseAddress = baseAddress })
        {
        }

        public void Dispose()
        {
            var httpClient = _httpClient;

            if (httpClient != null)
            {
                httpClient.Dispose();
                _httpClient = null;
            }

            return;
        }

        public void DisableAuthentication()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        /// <summary>
        /// http://www.iana.org/assignments/http-authschemes/http-authschemes.xhtml
        /// https://msdn.microsoft.com/en-us/library/ms789031(v=vs.110).aspx
        /// </summary>
        public void EnableAuthentication(string authorizationHeader)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationHeader);
            return;
        }

        /// <summary>
        /// http://tools.ietf.org/html/rfc7617
        /// </summary>
        public void EnableBasicAuthentication(string userName, string password)
        {
            var credentials = $"{userName}:{password}";
            var credentialBytes = Encoding.UTF8.GetBytes(credentials);
            var base64EncodedCredentials = Convert.ToBase64String(credentialBytes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedCredentials);
            return;
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc6750
        /// </summary>
        public void EnableBearerAuthentication(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return;
        }

        public void AddWebHookDeliveryId(string deliveryId)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-WebHook-DeliveryId", deliveryId);
            return;
        }

        public void AddWebHookEvent(string @event)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-WebHook-Event", @event);
            return;
        }

        //private string CalculatePayloadSignature(byte[] clientSecret, string json)
        //{
        //	var payload = Encoding.UTF8.GetBytes(json);
        //	byte[] hash;

        //	using (var hasher = new HMACSHA256(clientSecret))
        //	{
        //		hash = hasher.ComputeHash(payload);
        //	}

        //	return $"SHA256={hash.ToHexadecimalString()}";
        //}

        //public void AddWebHookSignature(byte[] clientSecret, string json)
        //{
        //	var payloadSignature = CalculatePayloadSignature(clientSecret, json);
        //	myHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-WebHook-Signature", payloadSignature);
        //	return;
        //}

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool ClientSecretsAreEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var areEqual = true;

            for (int index = 0, count = left.Length; index != count; ++index)
            {
                areEqual &= left[index] == right[index];
            }

            return areEqual;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool ClientSecretsAreEqual(string left, string right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var areEqual = true;

            for (int index = 0, count = left.Length; index != count; ++index)
            {
                areEqual &= left[index] == right[index];
            }

            return areEqual;
        }

        private static bool ValidateStatusCode(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthenticatedException();
                case HttpStatusCode.Forbidden:
                    throw new UnauthorizedAccessException();
                case HttpStatusCode.NotFound:
                    return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }

        private static async Task<TResult> ParseResponse<TResult>(HttpResponseMessage response)
        {
            var ok = ValidateStatusCode(response);

            if (!ok)
            {
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Converters = new[]
                {
                    new ValidationResultJsonConverter()
                }
            };

            return JsonConvert.DeserializeObject<TResult>(json, jsonSerializerSettings);
        }

        public async Task<TResult> GetAsync<TResult>(string relativeUrl)
        {
            using (var response = await _httpClient.GetAsync(new Uri(relativeUrl, UriKind.Relative)))
            {
                return await ParseResponse<TResult>(response);
            }
        }

        public async Task<TResult> PostAsync<TResult>(string relativeUrl, string json)
        {
            using (var content = new StringContent(json, Encoding.UTF8, MediaType.Json))
            using (var response = await _httpClient.PostAsync(new Uri(relativeUrl, UriKind.Relative), content))
            {
                return await ParseResponse<TResult>(response);
            }
        }

        public async Task<TResult> PostAsync<TResult>(string relativeUrl, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            return await PostAsync<TResult>(relativeUrl, json);
        }

        public async Task PostAsync(string relativeUrl, string json)
        {
            using (var content = new StringContent(json, Encoding.UTF8, MediaType.Json))
            using (var response = await _httpClient.PostAsync(new Uri(relativeUrl, UriKind.Relative), content))
            {
                var ok = ValidateStatusCode(response);

                if (!ok)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            return;
        }

        public async Task PostAsync(string relativeUrl, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            await PostAsync(relativeUrl, json);
            return;
        }

        public async Task<TResult> DeleteAsync<TResult>(string relativeUrl)
        {
            using (var response = await _httpClient.DeleteAsync(new Uri(relativeUrl, UriKind.Relative)))
            {
                return await ParseResponse<TResult>(response);
            }
        }
    }
}
