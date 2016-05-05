namespace Aggregator.Core.Interfaces
{
    public interface IScriptLibrary
    {
        void SendMail(string from, string to, string subject, string body);
    }
}