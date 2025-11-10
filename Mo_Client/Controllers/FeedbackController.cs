using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;

namespace Mo_Client.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FeedbackController> _logger;
        private const string ApiClientName = "MoApi";
        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

        public FeedbackController(IHttpClientFactory httpClientFactory, ILogger<FeedbackController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Feedback or /Feedback?productId=123 or /Feedback/123
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "productId")] long? productId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Feedback.Index called. productId={ProductId}, QueryString={Query}", productId, Request?.QueryString.ToString());

            // when productId is not provided show friendly message + empty model (no crash)
            if (!productId.HasValue || productId.Value <= 0)
            {
                TempData["FeedbackError"] = "ProductId không hợp lệ.";
                ViewData["ProductId"] = 0L;
                ViewData["ProfileAccountId"] = 0L;
                ViewData["ProfileAccountName"] = "Bạn";
                return View(new List<FeedbackVm>());
            }

            var pid = productId.Value;

            var client = _httpClientFactory.CreateClient(ApiClientName);
            var accessToken = Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var feedbacks = new List<FeedbackVm>();

            try
            {
                using var resp = await client.GetAsync($"api/feedback/{pid}", cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                    // Handle either { data: [...] } or raw array
                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                        {
                            feedbacks = JsonSerializer.Deserialize<List<FeedbackVm>>(dataEl.GetRawText(), _serializerOptions) ?? new();
                        }
                        else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            feedbacks = JsonSerializer.Deserialize<List<FeedbackVm>>(json, _serializerOptions) ?? new();
                        }
                        else if (doc.RootElement.TryGetProperty("Data", out var dataEl2) && dataEl2.ValueKind == JsonValueKind.Array)
                        {
                            feedbacks = JsonSerializer.Deserialize<List<FeedbackVm>>(dataEl2.GetRawText(), _serializerOptions) ?? new();
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Cannot parse feedback JSON; trying direct deserialization.");
                        try
                        {
                            feedbacks = JsonSerializer.Deserialize<List<FeedbackVm>>(json, _serializerOptions) ?? new();
                        }
                        catch (Exception inner)
                        {
                            _logger.LogError(inner, "Failed to deserialize feedback list.");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("API GET api/feedback/{ProductId} returned {StatusCode}", resp.StatusCode);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request cancelled while fetching feedbacks for product {ProductId}", pid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching feedbacks for product {ProductId}", pid);
            }

            // attempt to prefill profile info if available
            FeedbackVm profile = null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    profile = await GetProfileAsync(client, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to fetch profile while rendering feedback index.");
                }
            }

            ViewData["ProductId"] = pid;
            ViewData["ProfileAccountName"] = profile?.AccountName ?? "Bạn"; // use ViewData (view reads ViewData)
            TempData.Remove("FeedbackSuccess"); // do not set success when merely loading list  

            return View(feedbacks);
        }

        // GET: /Feedback/Create?productId=123
        [HttpGet]
        public async Task<IActionResult> Create(long productId, CancellationToken cancellationToken)
        {
            if (productId <= 0)
            {
                TempData["FeedbackError"] = "ProductId không hợp lệ.";
                return RedirectToAction("Index", new { productId = 0 });
            }

            var client = _httpClientFactory.CreateClient(ApiClientName);
            var accessToken = Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string profileName = "Bạn";
            long profileAccountId = 0;
            try
            {
                var profile = await GetProfileAsync(client, cancellationToken);
                if (profile != null)
                {
                    profileAccountId = profile.AccountId;
                    profileName = profile.AccountName ?? profileName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "GetProfile failed in Create()");
            }

            ViewData["ProductId"] = productId;
            ViewData["ProfileAccountId"] = profileAccountId;
            ViewData["ProfileAccountName"] = profileName;

            return View("creatfeedback");
        }

        // POST: /Feedback/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(long productId, int rating, string comment, CancellationToken cancellationToken)
        {
            // basic validation
            if (productId <= 0)
            {
                TempData["FeedbackError"] = "ProductId không hợp lệ.";
                return RedirectToAction("Create", new { productId });
            }

            rating = Math.Clamp(rating, 1, 5);
            if (!string.IsNullOrEmpty(comment) && comment.Length > 1000)
                comment = comment.Substring(0, 1000);

            var client = _httpClientFactory.CreateClient(ApiClientName);
            var accessToken = Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            long accountId = 0;
            try
            {
                var profile = await GetProfileAsync(client, cancellationToken);
                if (profile != null)
                    accountId = profile.AccountId;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to get profile before submitting feedback.");
            }

            var payload = new
            {
                AccountId = accountId,
                ProductId = productId,
                Rating = rating,
                Comment = comment
            };

            try
            {
                var json = JsonSerializer.Serialize(payload, _serializerOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await client.PostAsync("api/feedback", content, cancellationToken);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                    TempData["FeedbackError"] = $"Gửi feedback thất bại ({(int)resp.StatusCode}).";
                    _logger.LogWarning("POST api/feedback failed. Status: {Status}, Body: {Body}", resp.StatusCode, body);
                }
                else
                {
                    TempData["FeedbackSuccess"] = "Gửi feedback thành công. Cảm ơn bạn!";
                }
            }
            catch (OperationCanceledException)
            {
                TempData["FeedbackError"] = "Yêu cầu đã bị huỷ.";
                _logger.LogInformation("Submit feedback cancelled for product {ProductId}", productId);
            }
            catch (Exception ex)
            {
                TempData["FeedbackError"] = "Có lỗi khi gửi feedback.";
                _logger.LogError(ex, "Error when submitting feedback for product {ProductId}", productId);
            }

            return RedirectToAction("Index", new { productId });
        }

        // helper: fetch current profile (returns null on failure)
        private static async Task<FeedbackVm?> GetProfileAsync(HttpClient client, CancellationToken cancellationToken)
        {
            using var profileResp = await client.GetAsync("api/account/profile", cancellationToken);
            if (!profileResp.IsSuccessStatusCode)
                return null;

            var profileJson = await profileResp.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                using var doc = JsonDocument.Parse(profileJson);
                if (doc.RootElement.TryGetProperty("Data", out var dataEl))
                {
                    var vm = JsonSerializer.Deserialize<FeedbackVm>(dataEl.GetRawText(), _serializerOptions);
                    return vm;
                }

                // fallback: if the API returned the account directly
                var direct = JsonSerializer.Deserialize<FeedbackVm>(profileJson, _serializerOptions);
                return direct;
            }
            catch
            {
                return null;
            }
        }
    }
}



