using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugRobot.Lib
{
    public class TFSManager
    {
        public string CollectionUrl { get; set; }
        public string ProjectName { get; set; }

        private const string _ASSIGNED_TO = "Assigned To";

        public TFSManager(string collectionUrl, string projectName)
        {
            this.CollectionUrl = collectionUrl;
            this.ProjectName = projectName;
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

        private WorkItemCollection GetQueryItens(string queryName, Dictionary<string, string> variables)
        {
            var collectionUri = new Uri(CollectionUrl);
            var server = new TfsTeamProjectCollection(collectionUri);
            var workItemStore = server.GetService<WorkItemStore>();

            var teamProject = workItemStore.Projects[ProjectName];

            var x = teamProject.QueryHierarchy;
            var queryId = FindQuery(x, queryName);

            var queryDefinition = workItemStore.GetQueryDefinition(queryId);

            var result = workItemStore.Query(queryDefinition.QueryText, variables);

            return result;
        }

        public IEnumerable<WorkItem> GetUnassignedBugs(string queryName)
        {
            var variables = new Dictionary<string, string>()
            {
                { "project", ProjectName }
            };

            var queryBase = GetQueryItens(queryName, variables);

            var query = from WorkItem workitem in queryBase
                        where workitem.Type.Name.ToLower() == "bug" &&
                              workitem.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                             (workitem.Fields[_ASSIGNED_TO].Value == null ||
                              workitem.Fields[_ASSIGNED_TO].Value.ToString() == string.Empty)
                        select workitem;

            return query;
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

                    if(workItem.IsValid() && workItem.Fields[_ASSIGNED_TO].IsValid)
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
    }
}
