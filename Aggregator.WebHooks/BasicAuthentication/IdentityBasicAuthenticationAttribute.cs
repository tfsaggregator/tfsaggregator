namespace BasicAuthentication.Filters
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Aggregator.WebHooks.Utils;

    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        private readonly ILogEvents2 logger;

        public IdentityBasicAuthenticationAttribute(ILogEvents2 logger)
        {
            this.logger = logger;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string token = string.Empty;
            var usersCollection = ConfigurationManager.GetSection("Users") as NameValueCollection;
            if (usersCollection != null)
            {
                token = usersCollection[userName].ToString();
            }

            if (token != password)
            {
                logger.BasicAuthenticationFailed(userName);

                // No user with userName/password exists.
                return null;
            }

            logger.BasicAuthenticationSucceeded(userName);

            // Create a ClaimsIdentity with all the claims for this user.
            ClaimsIdentity identity = new ClaimsIdentity("Basic");
            identity.AddClaim(new Claim("username", userName));
            return new ClaimsPrincipal(identity);
        }
    }
}