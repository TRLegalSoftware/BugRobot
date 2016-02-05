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
        public bool NotifyOnlyNewBugs { get; set; }

        public BugRobot(string queryUrl, string userName, bool autoAssign, bool notifyOnlyNewBugs)
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
            this.NotifyOnlyNewBugs = notifyOnlyNewBugs;
        }

        public Message Run()
        {
            var hasBugIcon = "Content/bug_error.png";
            var hasNoBugIcon = "Content/bug_ok.png";

            Message message;

            var bugs = this.TfsManager.GetUnassignedBugs(this.QueryName);

            if(bugs.Count() > 0)
            {
                var firstBug = bugs.FirstOrDefault();

                if (this.AutoAssign)
                {
                    var success = this.TfsManager.AssignWorkItem(firstBug, this.UserName);

                    message = success ? new Message() { Title = string.Format("Novo bug em aberto:\n\n{0}\n{1}", firstBug.Id, firstBug.Title), Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true, Icon = hasBugIcon }
                                      : new Message() { Title = "Ocorreu um erro ao atribuir o bug ao desenvolvedor", Success = true, Icon = hasBugIcon };
                }
                else
                {
                    message = bugs.Count() == 1 ? new Message() { Title = string.Format("Novo bug em aberto:\n\n{0}\n{1}", firstBug.Id, firstBug.Title), Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true, Icon = hasBugIcon }
                                                : new Message() { Title = string.Format("{0} novos bugs em aberto:\n\n{1}", bugs.Count(), string.Join("\n", bugs.Select(s => s.Id))), Url = this.QueryURL, Success = true, Icon = hasBugIcon };
                }
            }
            else
            {
                message = new Message() { Title = "Nenhum bug em aberto", Success = !this.NotifyOnlyNewBugs, Icon = hasNoBugIcon };
            }

            return message;
        }

        public class Message
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public bool Success { get; set; }
            public string Icon { get; set; }
        }
    }

}
