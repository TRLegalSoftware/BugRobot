using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BugRobot.Lib
{
    public class BugRobot
    {
        public TFSManager TfsManager { get; set; }
        public string QueryURL { get; set; }
        public string QueryName { get; set; }
        public string UserName { get; set; }
        public bool AutoAssign { get; set; }

        public BugRobot(string queryUrl, string userName, bool autoAssign)
        {
            this.QueryURL = queryUrl;

            var url = new Uri(queryUrl);

            var queryString = HttpUtility.ParseQueryString(url.Fragment.Replace("#", ""));
            var queryName = HttpUtility.UrlDecode(queryString["path"]).Split('/').Last();

            var collectionIndex = url.AbsoluteUri.IndexOf("Collection", StringComparison.InvariantCultureIgnoreCase);
            var separationIndex = url.AbsoluteUri.IndexOf('/', collectionIndex);
            var projectIndex = url.AbsoluteUri.IndexOf('/', separationIndex + 1);

            var collectionUrl = url.AbsoluteUri.Substring(0, separationIndex);
            var projectName = url.AbsoluteUri.Substring(separationIndex + 1, projectIndex - separationIndex - 1);

            this.TfsManager = new TFSManager(collectionUrl, projectName);
            this.QueryName = queryName;
            this.UserName = userName;
            this.AutoAssign = autoAssign;
        }

        public Message Run()
        {
            Message message;

            var bugs = this.TfsManager.GetUnassignedBugs(this.QueryName);

            if(bugs.Count() > 0)
            {
                var firstBug = bugs.FirstOrDefault();

                if (this.AutoAssign)
                {
                    var success = this.TfsManager.AssignWorkItem(firstBug, this.UserName);

                    message = success ? new Message() { Title = firstBug.Title, Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true }
                                      : new Message() { Title = "Error on assigning bug", Success = false };
                }
                else
                {
                    message = bugs.Count() == 1 ? new Message() { Title = firstBug.Title, Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true }
                                                : new Message() { Title = string.Format("{0} New bugs available", bugs.Count()), Url = this.QueryURL, Success = false };
                }
            }
            else
            {
                message = new Message() { Title = "No bugs available", Success = false };
            }

            return message;
        }

        public class Message
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public bool Success { get; set; }
        }
    }

}
