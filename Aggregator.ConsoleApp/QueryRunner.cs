namespace Aggregator.ConsoleApp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    // derived from http://www.colinsalmcorner.com/post/using-the-tfs-api-to-display-results-of-a-hierarchical-work-item-query
    internal class QueryRunner
    {
        public class QueryResult
        {
            private IDictionary<int, WorkItem> workItems;
            private IList<WorkItemLinkInfo> links;

            // this is interesting when reasoning on links
            private QueryType queryType;

            public IDictionary<int, WorkItem> WorkItems { get => this.workItems; set => this.workItems = value; }

            public IList<WorkItemLinkInfo> Links { get => this.links; set => this.links = value; }

            public QueryType QueryType { get => this.queryType; set => this.queryType = value; }
        }

        public WorkItemStore WorkItemStore { get; private set; }

        public string TeamProjectName { get; private set; }

        public string CurrentUserDisplayName { get; private set; }

        public QueryRunner(WorkItemStore workItemStore, string teamProjectName)
        {
            this.WorkItemStore = workItemStore;
            this.TeamProjectName = teamProjectName;
        }

        public QueryResult RunQuery(string queryPathOrText)
        {
            string queryText;
#pragma warning disable S1854 // Dead stores should be removed
            var queryType = QueryType.Invalid;
#pragma warning restore S1854 // Dead stores should be removed

            if (queryPathOrText.StartsWith("SELECT ", StringComparison.InvariantCultureIgnoreCase))
            {
                // explicit query
                queryText = queryPathOrText;
                queryType = QueryType.List;
            }
            else
            {
                // name of existing query definition
                var rootQueryFolder = this.WorkItemStore.Projects[this.TeamProjectName].QueryHierarchy as QueryFolder;
                var queryDef = FindQuery(queryPathOrText, rootQueryFolder);

                // query not found
                if (queryDef == null)
                {
                    return null;
                }

                queryText = queryDef.QueryText;
                queryType = queryDef.QueryType;
            }

            // get the query
            var query = new Query(this.WorkItemStore, queryText, this.GetParamsDictionary());

            // run the query
            var dict = new Dictionary<int, WorkItem>();
            var list = new List<WorkItemLinkInfo>();
            if (queryType == QueryType.List)
            {
                foreach (WorkItem wi in query.RunQuery())
                {
                    dict.Add(wi.Id, wi);
                }
            }
            else
            {
                list.AddRange(query.RunLinkQuery());
                foreach (var k in list)
                {
                    if (k.SourceId != 0
                        && !dict.ContainsKey(k.SourceId))
                    {
                        dict.Add(k.SourceId, this.WorkItemStore.GetWorkItem(k.SourceId));
                    }

                    if (!dict.ContainsKey(k.TargetId))
                    {
                        dict.Add(k.TargetId, this.WorkItemStore.GetWorkItem(k.TargetId));
                    }
                }
            }

            return new QueryResult() { WorkItems = dict, Links = list, QueryType = queryType };
        }

        private static QueryDefinition FindQuery(string queryPath, QueryFolder queryFolder)
        {
            var parts = queryPath.Split('\\');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                queryFolder = queryFolder[parts[i]] as QueryFolder;
            }

            string queryName = parts[parts.Length - 1];
            if (!queryFolder.Contains(queryName))
            {
                return null;
            }

            var queryDef = queryFolder[queryName] as QueryDefinition;
            return queryDef;
        }

        private IDictionary GetParamsDictionary()
        {
            return new Dictionary<string, string>()
            {
                { "project", this.TeamProjectName },
                { "me", this.CurrentUserDisplayName }
            };
        }
    }
}
