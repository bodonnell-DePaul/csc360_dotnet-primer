using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
            string base64Credentials;
            string hashCreds = String.Empty;
            try{
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                base64Credentials = credentialBytes.ToString();
                var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];

                //byte[] creds = base64Credentials//Encoding.UTF8.GetBytes(base64Credentials);
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(credentialBytes);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                   hashCreds = builder.ToString();
                }
                
            }
            catch{
                return AuthenticateResult.Fail("Error decrypting login");
            }

            using(var db = new LoginContext())
            {
                var login = db.Logins.FirstOrDefault(l => l.Username == username);
                if(login == null){
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid username"));
                }
                if(login.Password != password){
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid password"));
                }
                // if(login.base64Credentials != base64Credentials){
                //     return await Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));
                // }
                if(login.hash != hashCreds){
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));
                }

                var claims = new System.Security.Claims.Claim[]{
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username)
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
        }
    }
}