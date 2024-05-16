using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace dotnet_primer
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(!Request.Headers.ContainsKey("Authorization")){
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            string username = String.Empty;
            string password = String.Empty;
            try{
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];
            }
            catch{
                return AuthenticateResult.Fail("Error decrypting login");
            }

            if(username == "brian" && password == "password"){
                var claims = new System.Security.Claims.Claim[]{
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username)
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
                // return AuthenticateResult.Success(new AuthenticationTicket(
                //     new System.Security.Claims.ClaimsPrincipal(), "BasicAuthentication"));
            }

            // Implement your authentication logic here.
            return await Task.FromResult(AuthenticateResult.Fail("Failed authentication"));
        }
    }
}