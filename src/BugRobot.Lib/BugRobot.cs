using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public string NotifiedBugs { get; set; }

        public BugRobot(string queryUrl, string userName, bool autoAssign, bool notifyOnlyNewBugs, string notifiedBugs)
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
            this.NotifiedBugs = notifiedBugs;
        }

        public Message Run()
        {
            var hasBugIcon = "Content/bug_error.png";
            var hasNoBugIcon = "Content/bug_ok.png";

            Message message;

            var bugs = this.TfsManager.GetUnassignedBugs(this.QueryName);

            var notifiedBugs = (!string.IsNullOrEmpty(this.NotifiedBugs)) 
                                ? this.NotifiedBugs.Split(',').Select(id => Convert.ToInt32(id)).ToList()
                                : new List<Int32>();

            var filteredBugs = bugs.Where(x => !notifiedBugs.Contains(x.Id));

            if (filteredBugs.Count() > 0)
            {
                var firstBug = filteredBugs.FirstOrDefault();

                if (this.AutoAssign)
                {
                    var success = this.TfsManager.AssignWorkItem(firstBug, this.UserName);

                    message = success ? new Message() { Title = string.Format("Novo bug em aberto:\n\n{0}\n{1}", firstBug.Id, firstBug.Title), Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true, Icon = hasBugIcon }
                                      : new Message() { Title = "Ocorreu um erro ao atribuir o bug ao desenvolvedor", Success = true, Icon = hasBugIcon };
                }
                else
                {
                    message = filteredBugs.Count() == 1 ? new Message() { Title = string.Format("Novo bug em aberto:\n\n{0}\n{1}", firstBug.Id, firstBug.Title), Url = this.TfsManager.GetWorkItemUrl(firstBug), Success = true, Icon = hasBugIcon }
                                                : new Message() { Title = string.Format("{0} novos bugs em aberto:\n\n{1}", filteredBugs.Count(), string.Join("\n", filteredBugs.Select(s => s.Id))), Url = this.QueryURL, Success = true, Icon = hasBugIcon };

                }

                foreach (var item in filteredBugs.Select(i => i.Id).ToList())	            
                    message.Ids += string.IsNullOrEmpty(message.Ids) ? item.ToString() : string.Concat(",", item.ToString());
            }
            else
            {
                if (bugs.Count() > 0)
                    message = new Message() { Title = "Bugs já avisados", Success = !this.NotifyOnlyNewBugs, Icon = hasNoBugIcon };
                else
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
            public string Ids { get; set; }
        }
    }

}
