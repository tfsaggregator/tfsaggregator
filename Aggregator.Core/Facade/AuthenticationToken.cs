using System;
using Microsoft.TeamFoundation.Client;

using IdentityDescriptor = Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor;

namespace Aggregator.Core.Facade
{
    public partial class WorkItemRepository
    {
        public abstract class AuthenticationToken
        {
            public abstract TfsTeamProjectCollection GetCollection(Uri tfsCollectionUri);

            public override string ToString()
            {
                return this.GetType().Name;
            }
        }

        public class WindowsIntegratedAuthenticationToken : AuthenticationToken
        {
            public WindowsIntegratedAuthenticationToken()
            {
            }

            public override TfsTeamProjectCollection GetCollection(Uri tfsCollectionUri)
            {
                return new TfsTeamProjectCollection(tfsCollectionUri);
            }
        }

        public class PersonalAuthenticationToken : AuthenticationToken
        {
            private readonly string personalToken;

            public PersonalAuthenticationToken(string personalToken)
            {
                this.personalToken = personalToken;
            }

            public override TfsTeamProjectCollection GetCollection(Uri tfsCollectionUri)
            {
                // username is not important, we use it to identify ourselves to callee
                var tfsCred = new TfsClientCredentials(
                    new BasicAuthCredential(
                        new System.Net.NetworkCredential(
                            "tfsaggregator2-webhooks", this.personalToken)));
                tfsCred.AllowInteractive = false;
                return new TfsTeamProjectCollection(tfsCollectionUri, tfsCred);
            }
        }

        public class BasicAuthenticationToken : AuthenticationToken
        {
            private readonly string username;
            private readonly string password;

            public BasicAuthenticationToken(string username, string password)
            {
                this.username = username;
                this.password = password;
            }

            public override TfsTeamProjectCollection GetCollection(Uri tfsCollectionUri)
            {
                var tfsCred = new TfsClientCredentials(
                    new BasicAuthCredential(
                        new System.Net.NetworkCredential(this.username, this.username)));
                tfsCred.AllowInteractive = false;
                return new TfsTeamProjectCollection(tfsCollectionUri, tfsCred);
            }

            public override string ToString()
            {
                return $"{base.ToString()}({this.username})";
            }
        }

        public class ImpersonateAuthenticationToken : AuthenticationToken
        {
            private readonly IdentityDescriptor identityDescriptor;

            public ImpersonateAuthenticationToken(IdentityDescriptor identityDescriptor)
            {
                this.identityDescriptor = identityDescriptor;
            }

            public override TfsTeamProjectCollection GetCollection(Uri tfsCollectionUri)
            {
                return new TfsTeamProjectCollection(tfsCollectionUri, this.identityDescriptor);
            }

            public override string ToString()
            {
                return $"{base.ToString()}({this.identityDescriptor.Identifier})";
            }
        }
    }
}
