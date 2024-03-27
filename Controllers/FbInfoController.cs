using CircitAuth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CircitAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FbInfoController : ControllerBase
    {
        private readonly IHttpClientFactory HttpClientFactory;


        public FbInfoController(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetClaimValue(ClaimTypes.UserData);

            if (userId != string.Empty)
            {
                var token = Request.Headers["AuthToken"].FirstOrDefault()?.Split(" ").Last();

                if (token == null)
                {
                    return Unauthorized();
                }

                var httpClient = HttpClientFactory.CreateClient("FacebookApi");

                var response = await httpClient.GetAsync("me?fields=first_name,last_name,email,id&access_token=" + token);

                if (response.IsSuccessStatusCode)
                {
                    var responseContentString = await response.Content.ReadAsStringAsync();
                    var fbUserInfo = JsonSerializer.Deserialize<FacebookUserInfo>(responseContentString);

                    return Ok(fbUserInfo);
                }
            }

            return Unauthorized();
        }


        private string GetClaimValue(string claimType)
        {
            var claim = HttpContext.User.FindFirst(claimType);

            if (claim != null)
            {
                return claim.Value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}