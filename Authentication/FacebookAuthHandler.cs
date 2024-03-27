using CircitAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CircitAuth.Authentication
{
    public class FacebookAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly FacebookSettings FacebookSettingsSect;


        public FacebookAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<FacebookSettings> optMonAppSettings) : base(options, logger, encoder, clock)
        {
            HttpClientFactory = httpClientFactory;
            FacebookSettingsSect = optMonAppSettings.CurrentValue;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var token = Request.Headers["AuthToken"].FirstOrDefault()?.Split(" ").Last();

                if (token == null)
                {
                    return AuthenticateResult.Fail("Missing Authorization Header");
                }

                var httpClient = HttpClientFactory.CreateClient("FacebookApi");

                var tokenResponse = await httpClient.GetAsync("debug_token?input_token=" + token + "&access_token=" + FacebookSettingsSect.AppId + "|" + FacebookSettingsSect.ClientSecret);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var responseContentString = await tokenResponse.Content.ReadAsStringAsync();
                    var fbUser = JsonSerializer.Deserialize<FacebookUser>(responseContentString);

                    if (fbUser?.data.is_valid == true)
                    {
                        List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.UserData, fbUser.data.user_id) };
                        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        var authenticationTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                        return AuthenticateResult.Success(authenticationTicket);
                    }
                    else
                    {
                        return AuthenticateResult.Fail("Authorization Failed");
                    }
                }
                else
                {
                    return AuthenticateResult.Fail("Authorization Failed");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Authorization Failed");
            }
        }
    }
}