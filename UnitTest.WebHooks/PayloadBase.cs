using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace UnitTest.WebHooks
{
    class WorkItemRequestSample : Aggregator.WebHooks.Models.WorkItemRequest { }

    public class PayloadBase
    {
        protected JObject GetDataFileMatchingCallerName([CallerMemberName]string methodName = "")
        {
            string area = this.GetType().Name.Split('ˆ')[1];
            // ˆ is U+02C6 Modifier Letter Circumflex Accent
            string testName = methodName.Split('ˆ')[0].Replace('_', '-');
            string dataFile = $@"PostData\{area}\{area}-workitem.{testName}.json";
            return JObject.Parse(File.ReadAllText(dataFile));
        }
    }
}
