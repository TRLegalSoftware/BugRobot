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
        public string BotName { get; set; }
        public bool AutoAssign { get; set; }
        public bool IsRunning { get; set; }

        private List<WorkItemLog> logList { get; set; }
        private INotification Notification;
        private string ContextString;

        /// <summary>
        /// Creates a new instance of a WorkItemRobot
        /// </summary>
        /// <param name="queryUrl">The wanted query to be scanned</param>
        /// <param name="contextString">A context string to be added to the message</param>
        /// <param name="notificationSystem">The notification system that'll be used</param>
        /// <param name="botName">A name, to be added to the log "BotName" column(Optional. Default "WorkItem")</param>
        /// <param name="userName">The user name(Optional. Default: Null)</param>
        /// <param name="autoAssign">True if wanted to auto assign the found work item to the username(Optional. Default: False)</param>
        public WorkItemRobot(
            string queryUrl,
            string contextString,
            INotification notificationSystem,
            string botName = "WorkItem",
            string userName = null,
            bool autoAssign = false)
        {
            this.IsRunning = false;
            this.QueryURL = queryUrl;
            this.ContextString = contextString;
            this.Notification = notificationSystem;
            this.BotName = botName;
            this.UserName = userName;
            this.AutoAssign = autoAssign;
            
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
            if (!IsRunning)
            {
                IsRunning = true;
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
                        var title = string.Format("Novo {0} {1}: {2}", wi.Type.Name, ContextString, wi.Id);
                        var url = this.WorkItemTfsManager.GetWorkItemUrl(wi);
                        var id = wi.Id;
                        var content = wi.Title;

                        this.CallWorkItem(title, url, content, id, true);
                    }
                    else
                    {
                        //If there's more than one bug

                        //Get a more generalized title
                        var title = string.Format(
                            "{0} novos {1} {2}: {3}",
                            //{0}How many
                            workItems.Count(),
                            //{1}Type name
                            workItems.LastOrDefault().Type.Name,
                            //{2}Type name
                            ContextString,
                            //{3}Its IDs
                            string.Join(", ", workItems.Select(s => s.Id))
                        );

                        string content;

                        //If the string is too long
                        if (string.Join("\n", workItems.Select(s => s.Title)).Length > 235)
                        {
                            //Shows only the first item and tell how many more items left
                            content = string.Format("{0}...\n+{1} {2}(s)",
                                //{0}First item title(cropped)
                                workItems.LastOrDefault().Title,
                                //{1}How many
                                workItems.Count()-1,
                                //{2}Type name
                                workItems.LastOrDefault().Type.Name
                            );
                        }
                        else
                        {
                            //If it's not too long show everything
                            content = string.Format(
                                "{0}",
                                string.Join("\n", workItems.Select(s => s.Title))
                            );
                        }

                        this.CallWorkItem(title, null, content);

                        foreach (WorkItem wi in workItems)
                        {
                            addTolist(wi.Title, null, wi.Id, true);
                        }
                    }
                }
                else
                {
                    Notification.callNotification(new NotificationContent()
                    {
                        Title = string.Format("Nenhum item {0}", ContextString)
                    });
                }
            }
            IsRunning = false;
            return logList;
        }

        private void CallWorkItem(string workItemTitle, string workItemUrl = null, string workItemContent = null, int? workItemId = null, bool isToAddList = false)
        {
            //Create a log to add in the history table
            var wiLog = addTolist(workItemContent ?? workItemTitle, workItemUrl, workItemId, isToAddList);

            var notificationContent = new NotificationContent()
            {
                Title = workItemTitle
            };

            if (!string.IsNullOrEmpty(wiLog.Url))
                notificationContent.Url = wiLog.Url;

            if (!string.IsNullOrEmpty(workItemContent))
                notificationContent.Content = workItemContent;

            //Call a notification
            Notification.callNotification(notificationContent);
        }

        private WorkItemLog addTolist(string logItemTitle, string logItemUrl = null, int? logItemId = null, bool isToAddList = false)
        {
            //Create a log to add in the history table
            var wiLog = new WorkItemLog()
            {
                BotName = BotName,
                Title = logItemTitle
            };

            if (!string.IsNullOrEmpty(logItemUrl))
                wiLog.Url = logItemUrl;
            if (logItemId != null)
                wiLog.Id = logItemId;

            if (isToAddList && !logList.Contains(wiLog))
                //Add to the log list
                logList.Add(wiLog);

            return wiLog;
        }
    }
}
