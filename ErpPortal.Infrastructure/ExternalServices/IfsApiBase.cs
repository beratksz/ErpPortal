using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace ErpPortal.Infrastructure.ExternalServices
{
    public abstract class IfsApiBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly IConfiguration _configuration;
        protected readonly string _baseUrl;
        protected readonly string _username;
        protected readonly string _password;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        protected IfsApiBase(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            _baseUrl = configuration["IfsApi:BaseUrl"] ?? throw new ArgumentNullException("IfsApi:BaseUrl");
            _username = configuration["IfsApi:Username"] ?? throw new ArgumentNullException("IfsApi:Username");
            _password = configuration["IfsApi:Password"] ?? throw new ArgumentNullException("IfsApi:Password");
            
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
        }

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content) ?? throw new Exception("Response was null");
            }
            catch (HttpRequestException ex)
            {
                throw new IfsApiException($"Error calling IFS API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new IfsApiException($"Error deserializing IFS API response: {ex.Message}", ex);
            }
        }

        protected async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent) ?? throw new Exception("Response was null");
            }
            catch (HttpRequestException ex)
            {
                throw new IfsApiException($"Error calling IFS API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new IfsApiException($"Error deserializing IFS API response: {ex.Message}", ex);
            }
        }

        protected async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions)!;
        }

        protected async Task<(T, string)> GetWithETagAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var etag = response.Headers.ETag?.Tag;
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions);

            if (result == null || etag == null)
            {
                throw new InvalidOperationException("API yanıtı veya ETag alınamadı.");
            }

            return (result, etag);
        }

        protected async Task<T> PatchAsync<T>(string endpoint, object data, string etag)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
            {
                Content = content
            };

            if (string.IsNullOrWhiteSpace(etag) || etag == "*")
            {
                request.Headers.TryAddWithoutValidation("If-Match", "*");
            }
            else
            {
                request.Headers.TryAddWithoutValidation("If-Match", etag);
            }

            request.Headers.Add("Prefer", "return=minimal");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API Patch isteği başarısız: {response.StatusCode}. Detay: {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                // PATCH yanıtı boş dönebilir, bu durumda varsayılan bir T döndür.
                return default(T)!;
            }

            return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions)!;
        }
    }

    public class IfsApiException : Exception
    {
        public IfsApiException(string message) : base(message)
        {
        }

        public IfsApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 