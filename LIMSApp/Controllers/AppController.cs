using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;

namespace LIMSApp.Controllers
{
    public class AppController : Controller
    {
        private readonly ILogger<AppController> _logger;

        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public AppController(
            IHttpClientFactory httpFactory, IConfiguration config, ILogger<AppController> logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;//DI
        }

        public IActionResult Index()
        {
            String name = "Title";
            ViewBag.Name = name;
            return View();
        }

        // Proxy strumienia MJPEG z kamery IP.
        // Adres bierzemy z konfiguracji (nie od użytkownika), żeby uniknąć SSRF.
        [HttpGet]
        public async Task Stream(CancellationToken ct)
        {
            var url = _config["Camera:StreamUrl"];
            if (string.IsNullOrWhiteSpace(url))
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            var client = _httpFactory.CreateClient("camera");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var user = _config["Camera:User"];
            if (!string.IsNullOrEmpty(user))
            {
                var token = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{user}:{_config["Camera:Password"]}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
            }

            try
            {
                using var upstream = await client.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!upstream.IsSuccessStatusCode)
                {
                    Response.StatusCode = (int)upstream.StatusCode;
                    return;
                }

                // Przekazujemy dalej Content-Type, np. multipart/x-mixed-replace; boundary=...
                Response.ContentType = upstream.Content.Headers.ContentType?.ToString()
                                       ?? "multipart/x-mixed-replace; boundary=frame";
                Response.Headers.CacheControl = "no-store";

                await using var source = await upstream.Content.ReadAsStreamAsync(ct);
                await source.CopyToAsync(Response.Body, 16 * 1024, ct);
            }
            catch (OperationCanceledException)
            {
                // Użytkownik zamknął stronę lub kliknął „Rozłącz” – to normalne.
            }
            catch (HttpRequestException)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
            }
        }
    }
}
