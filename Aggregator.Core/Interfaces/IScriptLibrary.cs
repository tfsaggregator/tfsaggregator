namespace Aggregator.Core.Interfaces
{
    public interface IScriptLibrary
    {
        void SendMail(string to, string subject, string body);

        string GetEmailAddress(string user, string defaultValue);
    }
}