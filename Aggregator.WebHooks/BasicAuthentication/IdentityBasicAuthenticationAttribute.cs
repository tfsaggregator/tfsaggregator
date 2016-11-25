using System.Collections.Specialized;
using System.Configuration;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace BasicAuthentication.Filters
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {

            string token = string.Empty;
            var usersCollection = ConfigurationManager.GetSection("Users") as NameValueCollection;
            if (usersCollection != null)
            {
                token = usersCollection[userName].ToString();
            }


            if (token != password)
            {
                // No user with userName/password exists.
                return null;
            }

            // Create a ClaimsIdentity with all the claims for this user.
            ClaimsIdentity identity = new ClaimsIdentity("Basic");
            identity.AddClaim(new Claim("username", userName));
            return new ClaimsPrincipal(identity);
        }
    }
}