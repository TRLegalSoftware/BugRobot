using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSRobot
{
    public class WorkItemTFSManager
    {
        public string CollectionUrl { get; set; }
        public string ProjectName { get; set; }

        private const string _ASSIGNED_TO = "Assigned To";

        public WorkItemTFSManager(string collectionUrl, string projectName)
        {
            this.CollectionUrl = collectionUrl;
            this.ProjectName = projectName;
        }

        /// <summary>
        /// Find work items inside the query you passed without any filter.\n
        /// Use GetWorkItems(queryName, filter) to filter this result.
        /// </summary>
        /// <param name="queryName">The query name</param>
        public IEnumerable<WorkItem> GetWorkItems(string queryName)
        {
            return this.GetWorkItems(queryName, null);
        }

        /// <summary>
        /// Find work items inside the query you passed that match the filter
        /// </summary>
        /// <param name="queryName">The query name</param>
        /// <param name="filter">Filter to get specific work items</param>
        public IEnumerable<WorkItem> GetWorkItems(string queryName, Func<WorkItem, bool> filter)
        {
            //In this context
            var context = new Dictionary<string, string>()
            {
                { "project", ProjectName }
            };

            //Get itens from some query inside the context
            var query = GetQueryItems(queryName, context);

            //Filter the query using a lambda to get only the items you want
            if (filter != null)
            {
                //Just filters if there's a filter
                var filteredQuery = (from WorkItem wi in query
                                     select wi).Where(filter);

                return filteredQuery;
            }
            else
            {
                //If there isn't a filter
                return (from WorkItem wi in query select wi);
            }
        }

        private WorkItemCollection GetQueryItems(string queryName, Dictionary<string, string> context)
        {
            //Get the collection based on the collection url generated when creating this class
            var collectionUri = new Uri(CollectionUrl);
            var server = new TfsTeamProjectCollection(collectionUri);

            //Get the work item store
            var _wis = new WorkItemStore(server);

            //Recover the project from this work item store
            var teamProject = _wis.Projects[ProjectName];

            //Get this query's ID
            var queryHier = teamProject.QueryHierarchy;
            var queryId = FindQuery(queryHier, queryName);

            //Get its definition
            var queryDef = _wis.GetQueryDefinition(queryId);

            //Return  the result
            var result = _wis.Query(queryDef.QueryText, context);            
            return result;

        }

        public bool AssignWorkItem(WorkItem workItem, string userName)
        {
            bool success = false;

            // Save old assignet to value to rollback
            string oldAssignedTo = (string)workItem.Fields[_ASSIGNED_TO].Value;

            // Try to save the work item 
            try
            {
                // Check if workitem is valid
                if (!workItem.IsDirty)
                {
                    workItem.PartialOpen();

                    workItem.Fields[_ASSIGNED_TO].Value = userName;

                    if (workItem.IsValid() && workItem.Fields[_ASSIGNED_TO].IsValid)
                    {
                        workItem.Save();
                        success = true;
                    }
                }
            }
            catch (ValidationException)
            {
                workItem.Fields[_ASSIGNED_TO].Value = oldAssignedTo;
                success = false;
            }
            finally
            {
                workItem.Close();
            }

            return success;
        }

        public string GetWorkItemUrl(WorkItem workItem)
        {
            var stringUrl = string.Format("{0}/{1}/_workitems#_a=edit&id={2}&triage=true", this.CollectionUrl, this.ProjectName, workItem.Id);

            return new Uri(stringUrl).AbsoluteUri;
        }

        private Guid FindQuery(QueryFolder folder, string queryName)
        {
            foreach (var item in folder)
            {
                if (item.Name.Equals(queryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item.Id;
                }

                var itemFolder = item as QueryFolder;
                if (itemFolder != null)
                {
                    var result = FindQuery(itemFolder, queryName);
                    if (!result.Equals(Guid.Empty))
                    {
                        return result;
                    }
                }
            }
            return Guid.Empty;
        }
    }
}
