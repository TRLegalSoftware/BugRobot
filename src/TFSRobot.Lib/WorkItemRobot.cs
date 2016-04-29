using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TFSRobot
{
    public class WorkItemRobot
    {
        public WorkItemTFSManager WorkItemTfsManager { get; set; }
        public string QueryURL { get; set; }
        public string QueryName { get; set; }
        public string UserName { get; set; }
        public bool AutoAssign { get; set; }

        private bool isRunning;
        private List<WorkItemLog> logList { get; set; }
        private INotification Notification;

        public WorkItemRobot(
            string queryUrl,
            string userName,
            bool autoAssign,
            INotification notificationSystem)
        {
            this.isRunning = false;
            this.QueryURL = queryUrl;
            this.UserName = userName;
            this.AutoAssign = autoAssign;
            this.Notification = notificationSystem;
            logList = new List<WorkItemLog>();

            #region Preparing query and items names to give it to TFS

            var url = new Uri(queryUrl);

            //Gets the query string from the query url;
            var queryString = HttpUtility.ParseQueryString(url.Fragment.Replace("#", ""));
            //Gets only the query name
            var queryName = HttpUtility.UrlDecode(queryString["path"]).Split('/').Last();
            this.QueryName = queryName;

            var collectionIndex = url.AbsoluteUri.IndexOf("Collection", StringComparison.InvariantCultureIgnoreCase);
            var separationIndex = url.AbsoluteUri.IndexOf('/', collectionIndex);
            var projectIndex = url.AbsoluteUri.IndexOf('/', separationIndex + 1);

            var collectionUrl = url.AbsoluteUri.Substring(0, separationIndex);
            var projectName = url.AbsoluteUri.Substring(separationIndex + 1, projectIndex - separationIndex - 1);

            #endregion

            this.WorkItemTfsManager = new WorkItemTFSManager(collectionUrl, projectName);
        }

        public IEnumerable<WorkItemLog> Run(Func<WorkItem, bool> filter)
        {
            if (!isRunning)
            {
                isRunning = true;
                var workItems = this.WorkItemTfsManager.GetWorkItems(this.QueryName, filter);

                //If returned workItems
                if (workItems.Count() > 0)
                {
                    //If this workItem will be assigned to this user name
                    if (this.AutoAssign)
                    {
                        foreach (WorkItem wi in workItems)
                        {
                            //Assign this WorkItem to the user
                            var success = this.WorkItemTfsManager.AssignWorkItem(wi, this.UserName);

                            //If not succeed
                            if (!success)
                            {
                                var title = string.Empty;
                                var url = this.WorkItemTfsManager.GetWorkItemUrl(wi);
                                var id = wi.Id;
                                var content = wi.Title;
                                title = string.Format("Ocorreu um erro ao atribuir o {1} ao desenvolvedor\n{1}: {0}", wi.Id, wi.Type.Name);
                                this.CallWorkItem(title, url, content, id);
                            }
                        }
                    }

                    //After assign, it'll show a message
                    if (workItems.Count() == 1)
                    {
                        //If there's only one bug
                        var wi = workItems.FirstOrDefault();
                        //If there's only one new bug, show it
                        //Create a log to add in the history table
                        var title = string.Format("Novo {1} em aberto: {0}", wi.Id, wi.Type.Name);
                        var url = this.WorkItemTfsManager.GetWorkItemUrl(wi);
                        var id = wi.Id;
                        var content = wi.Title;

                        this.CallWorkItem(title, url, content, id);
                    }
                    else
                    {
                        //If there's more than one bug

                        //Get a more generalized title
                        var title = string.Format(
                            "{0} novos {1} em aberto:\n\n{2}",
                            //{0}How many
                            workItems.Count(),
                            //{1}Type name
                            workItems.FirstOrDefault().Type.Name,
                            //{2}Its IDs
                            string.Join("\n", workItems.Select(s => s.Id))
                        );

                        this.CallWorkItem(title);
                    }
                }
                else
                {
                    //If there's no bug, there's no need to add it to log list, so it'll only show
                    //a notification
                    Notification.callNotification(new NotificationContent()
                    {
                        Title = "Nenhum item em aberto"
                    });
                }

                isRunning = false;
                
            }

            return logList;
        }


        private void CallWorkItem(string workItemTitle, string workItemUrl = null, string workItemContent = null, int? workItemId = null)
        {
            //Create a log to add in the history table
            var wiLog = new WorkItemLog()
            {
                Title = workItemTitle
            };

            if (!string.IsNullOrEmpty(workItemUrl))
                wiLog.Url = workItemUrl;
            if (workItemId != null)
                wiLog.Id = workItemId;

            var notificationContent = new NotificationContent()
            {
                Title = wiLog.Title
            };

            if (!string.IsNullOrEmpty(wiLog.Url))
                notificationContent.Url = wiLog.Url;

            //Add to the log list
            logList.Add(wiLog);
            //Call a notification
            Notification.callNotification(notificationContent);
        }
    }
}
