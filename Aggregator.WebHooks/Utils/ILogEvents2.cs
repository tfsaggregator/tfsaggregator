namespace Aggregator.WebHooks.Utils
{
    public interface ILogEvents2
    {
        void BasicAuthenticationSucceeded(string userName);

        void BasicAuthenticationFailed(string userName);
    }
}