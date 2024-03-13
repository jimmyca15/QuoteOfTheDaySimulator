using System.Net;
using System.Web;

namespace QuoteOfTheDaySimulator
{
    class QuoteOfTheDayClient : IDisposable
    {
        private readonly HttpClient _client;

        public QuoteOfTheDayClient()
        {
            _client = new HttpClient(
                new HttpClientHandler
                {
                    CookieContainer = new CookieContainer()
                },
                disposeHandler: true);
        }

        public async Task RegisterUser(string username, string password, CancellationToken cancellationToken)
        {
            using HttpResponseMessage registerPageResponse = await _client.GetAsync("https://localhost:7206/Identity/Account/Register", cancellationToken);

            string content = await registerPageResponse.Content.ReadAsStringAsync();

            string requestVerificationToken = GetHtmlElementAttributeValue(content, "__RequestVerificationToken", "value");

            IEnumerable<KeyValuePair<string, string>> registerParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Input.Email", username),
                new KeyValuePair<string, string>("Input.Password", password),
                new KeyValuePair<string, string>("Input.ConfirmPassword", password),
                new KeyValuePair<string, string>("__RequestVerificationToken", requestVerificationToken)
            };

            HttpResponseMessage registerResponse = await _client.PostAsync(
                "https://localhost:7206/Identity/Account/Register",
                new FormUrlEncodedContent(registerParameters),
                cancellationToken);

            HttpResponseMessage activateRegistrationResponse = await _client.GetAsync(
                $"https://localhost:7206/Identity/Account/RegisterConfirmation?email={username}&returnUrl=/",
                cancellationToken);

            string activationLink = GetHtmlElementAttributeValue(
                await activateRegistrationResponse.Content.ReadAsStringAsync(),
                "confirm-link",
                "href");

            HttpResponseMessage activateResponse = await _client.GetAsync(HttpUtility.HtmlDecode(activationLink), cancellationToken);
        }

        public async Task Login(string username, string password, CancellationToken cancellationToken)
        {
            using HttpResponseMessage loginPageResponse = await _client.GetAsync("https://localhost:7206/Identity/Account/Register", cancellationToken);

            string content = await loginPageResponse.Content.ReadAsStringAsync(cancellationToken);

            string requestVerificationToken = GetHtmlElementAttributeValue(content, "__RequestVerificationToken", "value");

            IEnumerable<KeyValuePair<string, string>> registerParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Input.Email", username),
                new KeyValuePair<string, string>("Input.Password", password),
                new KeyValuePair<string, string>("Input.RememberMe", "false"),
                new KeyValuePair<string, string>("__RequestVerificationToken", requestVerificationToken)
            };

            HttpResponseMessage loginResponse = await _client.PostAsync(
                "https://localhost:7206/Identity/Account/Login",
                new FormUrlEncodedContent(registerParameters),
                cancellationToken);
        }

        public async Task<Variant> GetVariant(CancellationToken cancellationToken)
        {
            using HttpResponseMessage mainPage = await _client.GetAsync("https://localhost:7206/", cancellationToken);

            string content = await mainPage.Content.ReadAsStringAsync(cancellationToken);

            return content.Contains("hope this makes your day") ?
                Variant.On :
                Variant.Off;
        }

        public async Task LikeQuote(CancellationToken cancellationToken)
        {
            using HttpResponseMessage mainPage = await _client.GetAsync("https://localhost:7206/", cancellationToken);

            string content = await mainPage.Content.ReadAsStringAsync(cancellationToken);

            string requestVerificationToken = GetHtmlElementAttributeValue(content, "__RequestVerificationToken", "value");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7206/Index?handler=HeartQuote&username=userb@contoso.com");

            request.Headers.TryAddWithoutValidation("RequestVerificationToken", requestVerificationToken);

            using HttpResponseMessage likeResponse = await _client.SendAsync(request, cancellationToken);
        }

        public async Task Logout(CancellationToken cancellationToken)
        {
            using HttpResponseMessage mainPage = await _client.GetAsync("https://localhost:7206/", cancellationToken);

            string content = await mainPage.Content.ReadAsStringAsync(cancellationToken);

            string requestVerificationToken = GetHtmlElementAttributeValue(content, "__RequestVerificationToken", "value");

            IEnumerable<KeyValuePair<string, string>> registerParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", requestVerificationToken)
            };

            HttpResponseMessage loginResponse = await _client.PostAsync(
                "https://localhost:7206/Identity/Account/Logout?returnUrl=/",
                new FormUrlEncodedContent(registerParameters),
                cancellationToken);
        }

        private string GetHtmlElementAttributeValue(string html, string elementHint, string attributeName)
        {
            string attributeMarker = $"{attributeName}=\"";

            int hintIndex = html.IndexOf(elementHint);

            int attributeMarkerIndex = html.IndexOf(attributeMarker, hintIndex);

            int endAttributeMarkerIndex = html.IndexOf("\"", attributeMarkerIndex + attributeMarker.Length);

            return html.Substring(attributeMarkerIndex + attributeMarker.Length, endAttributeMarkerIndex - (attributeMarkerIndex + attributeMarker.Length));
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
